using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Infrastructure.Persistence.Outbox;

namespace ModularSaaS.Infrastructure.Persistence.Interceptors;

internal sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDomainEventsToOutboxMessages(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        if (entities.Count == 0)
        {
            return;
        }

        var outboxMessages = new List<OutboxMessage>();

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                var eventType = domainEvent.GetType();
                var typeName = eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name;
                var content = JsonSerializer.Serialize(domainEvent, eventType, SerializerOptions);

                outboxMessages.Add(new OutboxMessage
                {
                    Id = domainEvent.EventId != Guid.Empty ? domainEvent.EventId : Guid.NewGuid(),
                    OccurredOnUtc = domainEvent.OccurredOnUtc,
                    Type = typeName,
                    Content = content,
                    ProcessedOnUtc = null,
                    Error = null,
                    RetryCount = 0
                });
            }

            entity.ClearDomainEvents();
        }

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
