using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Infrastructure.Persistence.Outbox;

internal sealed class OutboxProcessorBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxProcessorOptions> options,
    ILogger<OutboxProcessorBackgroundService> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly ConcurrentDictionary<string, Type?> TypeCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandlerMethodCache = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor background service started.");

        try
        {
            await ProcessBatchAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Unexpected error occurred during initial outbox processing.");
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }

                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Unexpected error occurred during outbox processing cycle.");
            }
        }

        logger.LogInformation("Outbox processor background service stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var batchSize = options.Value.BatchSize;
        var maxRetryCount = options.Value.MaxRetryCount;

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < maxRetryCount)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(stoppingToken);

        if (messages.Count == 0)
        {
            return;
        }

        logger.LogDebug("Fetched {Count} outbox messages for processing.", messages.Count);

        foreach (var message in messages)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await DispatchMessageAsync(message, stoppingToken);
                message.ProcessedOnUtc = clock.UtcNow;
                message.Error = null;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                message.RetryCount++;
                var errorText = ex.ToString();
                message.Error = errorText.Length > 4000 ? errorText[..4000] : errorText;

                logger.LogError(
                    ex,
                    "Failed to dispatch outbox message {MessageId} of type '{MessageType}'. Attempt {RetryCount}/{MaxRetries}.",
                    message.Id,
                    message.Type,
                    message.RetryCount,
                    maxRetryCount);
            }
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    private async Task DispatchMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var eventType = ResolveEventType(message.Type)
            ?? throw new InvalidOperationException($"Could not resolve domain event type '{message.Type}' for outbox message {message.Id}.");

        var domainEvent = JsonSerializer.Deserialize(message.Content, eventType, SerializerOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize outbox message {message.Id} to type '{eventType.FullName}'.");

        using var messageScope = scopeFactory.CreateScope();
        var tenantSetter = messageScope.ServiceProvider.GetRequiredService<ITenantSetter>();

        if (domainEvent is ITenantEvent tenantEvent)
        {
            tenantSetter.SetTenant(tenantEvent.TenantId);
        }
        else
        {
            tenantSetter.SetTenant(null, isPlatformScope: true);
        }

        var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = messageScope.ServiceProvider.GetServices(handlerInterfaceType).ToList();

        if (handlers.Count == 0)
        {
            logger.LogWarning(
                "No registered handlers found for domain event type '{EventType}' on outbox message {MessageId}.",
                eventType.FullName,
                message.Id);
            return;
        }

        var handleMethod = HandlerMethodCache.GetOrAdd(handlerInterfaceType, t =>
            t.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
            ?? throw new InvalidOperationException($"Method HandleAsync not found on {t.FullName}."));

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            var task = (Task?)handleMethod.Invoke(handler, [domainEvent, cancellationToken]);
            if (task is not null)
            {
                await task;
            }
        }
    }

    private static Type? ResolveEventType(string typeName)
    {
        return TypeCache.GetOrAdd(typeName, name =>
        {
            var type = Type.GetType(name, throwOnError: false);
            if (type is not null)
            {
                return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type is not null)
                {
                    return type;
                }
            }

            var simpleName = name.Split(',')[0].Trim();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(simpleName)
                    ?? assembly.GetTypes().FirstOrDefault(t => string.Equals(t.FullName, simpleName, StringComparison.Ordinal) ||
                                                               string.Equals(t.Name, simpleName, StringComparison.Ordinal));
                if (type is not null)
                {
                    return type;
                }
            }

            return null;
        });
    }
}
