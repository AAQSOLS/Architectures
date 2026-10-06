using System.Text.Json.Serialization;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity.Events;

[method: JsonConstructor]
public sealed record UserPermissionChangedDomainEvent(
    Guid UserId,
    Guid TenantId,
    Guid PermissionId,
    bool IsGranted,
    Guid EventId,
    DateTimeOffset OccurredOnUtc) : ITenantEvent
{
    public UserPermissionChangedDomainEvent(Guid userId, Guid tenantId, Guid permissionId, bool isGranted)
        : this(userId, tenantId, permissionId, isGranted, Guid.NewGuid(), TimeProvider.System.GetUtcNow())
    {
    }
}
