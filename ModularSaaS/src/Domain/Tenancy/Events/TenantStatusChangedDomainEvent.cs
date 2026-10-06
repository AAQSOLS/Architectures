using System.Text.Json.Serialization;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Domain.Tenancy.Events;

[method: JsonConstructor]
public sealed record TenantStatusChangedDomainEvent(
    Guid TenantId,
    TenantStatus Status,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : ITenantEvent
{
    public TenantStatusChangedDomainEvent(Guid tenantId, TenantStatus status)
        : this(tenantId, status, Guid.NewGuid(), TimeProvider.System.GetUtcNow())
    {
    }
}
