using System.Text.Json.Serialization;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity.Events;

[method: JsonConstructor]
public sealed record RolePermissionsUpdatedDomainEvent(
    Guid RoleId,
    Guid TenantId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : ITenantEvent
{
    public RolePermissionsUpdatedDomainEvent(Guid roleId, Guid tenantId)
        : this(roleId, tenantId, Guid.NewGuid(), TimeProvider.System.GetUtcNow())
    {
    }
}
