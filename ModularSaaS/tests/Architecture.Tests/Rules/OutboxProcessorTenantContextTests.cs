using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity.Events;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Infrastructure.Persistence.Outbox;
using ModularSaaS.Infrastructure.Tenancy;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class OutboxProcessorTenantContextTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly MethodInfo DispatchMethod = typeof(OutboxProcessorBackgroundService)
        .GetMethod("DispatchMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public async Task Dispatched_ITenantEvent_Sets_TenantContext_In_Handler_Scope()
    {
        var expectedTenantId = Guid.NewGuid();
        var capturedTenantId = Guid.Empty;
        var capturedIsPlatform = true;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantSetter>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<IDomainEventHandler<UserCreatedDomainEvent>>(sp =>
            new SpyTenantEventHandler((tenantId, isPlatform) =>
            {
                capturedTenantId = tenantId;
                capturedIsPlatform = isPlatform;
            }, sp.GetRequiredService<ITenantContext>()));

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var options = Options.Create(new OutboxProcessorOptions());
        var logger = NullLogger<OutboxProcessorBackgroundService>.Instance;

        var processor = new OutboxProcessorBackgroundService(scopeFactory, options, logger);

        var domainEvent = new UserCreatedDomainEvent(
            Guid.NewGuid(),
            expectedTenantId,
            "test@example.com",
            "John",
            "Doe");

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTimeOffset.UtcNow,
            Type = typeof(UserCreatedDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent, SerializerOptions)
        };

        var task = (Task)DispatchMethod.Invoke(processor, [message, CancellationToken.None])!;
        await task;

        Assert.Equal(expectedTenantId, capturedTenantId);
        Assert.False(capturedIsPlatform);
    }

    [Fact]
    public async Task Dispatched_NonTenantEvent_Sets_PlatformScope_In_Handler_Scope()
    {
        Guid? capturedTenantId = Guid.NewGuid();
        var capturedIsPlatform = false;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantSetter>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<IDomainEventHandler<TestPlatformDomainEvent>>(sp =>
            new SpyPlatformEventHandler((tenantId, isPlatform) =>
            {
                capturedTenantId = tenantId;
                capturedIsPlatform = isPlatform;
            }, sp.GetRequiredService<ITenantContext>()));

        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var options = Options.Create(new OutboxProcessorOptions());
        var logger = NullLogger<OutboxProcessorBackgroundService>.Instance;

        var processor = new OutboxProcessorBackgroundService(scopeFactory, options, logger);

        var domainEvent = new TestPlatformDomainEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTimeOffset.UtcNow,
            Type = typeof(TestPlatformDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent, SerializerOptions)
        };

        var task = (Task)DispatchMethod.Invoke(processor, [message, CancellationToken.None])!;
        await task;

        Assert.Null(capturedTenantId);
        Assert.True(capturedIsPlatform);
    }

    public sealed record TestPlatformDomainEvent(Guid EventId, DateTimeOffset OccurredOnUtc) : IDomainEvent;

    private sealed class SpyTenantEventHandler(Action<Guid, bool> onHandled, ITenantContext tenantContext)
        : IDomainEventHandler<UserCreatedDomainEvent>
    {
        public Task HandleAsync(UserCreatedDomainEvent domainEvent, CancellationToken ct = default)
        {
            onHandled(tenantContext.TenantId ?? Guid.Empty, tenantContext.IsPlatformScope);
            return Task.CompletedTask;
        }
    }

    private sealed class SpyPlatformEventHandler(Action<Guid?, bool> onHandled, ITenantContext tenantContext)
        : IDomainEventHandler<TestPlatformDomainEvent>
    {
        public Task HandleAsync(TestPlatformDomainEvent domainEvent, CancellationToken ct = default)
        {
            onHandled(tenantContext.TenantId, tenantContext.IsPlatformScope);
            return Task.CompletedTask;
        }
    }
}
