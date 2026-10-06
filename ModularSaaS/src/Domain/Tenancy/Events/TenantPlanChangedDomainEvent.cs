using System.Text.Json.Serialization;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Domain.Tenancy.Events;

[method: JsonConstructor]
public sealed record TenantPlanChangedDomainEvent(
    Guid TenantId,
    TenantPlan Plan,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : ITenantEvent
{
    public TenantPlanChangedDomainEvent(Guid tenantId, TenantPlan plan)
        : this(tenantId, plan, Guid.NewGuid(), TimeProvider.System.GetUtcNow())
    {
    }
}
