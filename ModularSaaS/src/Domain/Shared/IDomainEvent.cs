namespace ModularSaaS.Domain.Shared;

public interface IDomainEvent
{
    public Guid EventId { get; }

    public DateTimeOffset OccurredOnUtc { get; }
}
