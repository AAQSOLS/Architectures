#pragma warning disable SA1649 // StyleCop metadata convention allows standard generic interface file naming

using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Application.Shared.Abstractions;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    public Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
