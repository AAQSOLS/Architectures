namespace ModularSaaS.Domain.Shared;

public interface ITenantEvent : IDomainEvent
{
    public Guid TenantId { get; }
}
