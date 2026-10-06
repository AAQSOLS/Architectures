using System.Text.Json.Serialization;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity.Events;

[method: JsonConstructor]
public sealed record UserRoleAssignedDomainEvent(
    Guid UserId,
    Guid TenantId,
    Guid RoleId,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : ITenantEvent
{
    public UserRoleAssignedDomainEvent(Guid userId, Guid tenantId, Guid roleId)
        : this(userId, tenantId, roleId, Guid.NewGuid(), TimeProvider.System.GetUtcNow())
    {
    }
}
