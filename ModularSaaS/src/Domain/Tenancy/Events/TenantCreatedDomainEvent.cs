using System.Text.Json.Serialization;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Domain.Tenancy.Events;

[method: JsonConstructor]
public sealed record TenantCreatedDomainEvent(
    Guid TenantId,
    string Identifier,
    TenantPlan Plan,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : ITenantEvent
{
    public TenantCreatedDomainEvent(Guid tenantId, string identifier, TenantPlan plan)
        : this(tenantId, identifier, plan, Guid.NewGuid(), TimeProvider.System.GetUtcNow())
    {
    }
}
