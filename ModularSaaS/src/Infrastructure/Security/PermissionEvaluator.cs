using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Security.Authorization;

namespace ModularSaaS.Infrastructure.Security;

internal sealed class PermissionEvaluator(
    IPermissionCacheReader permissionCacheReader,
    ITenantContext tenantContext,
    ModularSaaS.Application.Shared.Abstractions.ICurrentUser currentUser) : IPermissionEvaluator
{
    public async ValueTask<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(permission) || !currentUser.UserId.HasValue || !tenantContext.TenantId.HasValue)
        {
            return false;
        }

        var permissions = await permissionCacheReader.GetPermissionsAsync(tenantContext.TenantId.Value, currentUser.UserId.Value, ct);
        return permissions.Contains(permission);
    }

    public async ValueTask<bool> HasAnyPermissionAsync(IReadOnlyCollection<string> permissions, CancellationToken ct = default)
    {
        if (permissions.Count == 0 || !currentUser.UserId.HasValue || !tenantContext.TenantId.HasValue)
        {
            return false;
        }

        var cached = await permissionCacheReader.GetPermissionsAsync(tenantContext.TenantId.Value, currentUser.UserId.Value, ct);
        return permissions.Any(cached.Contains);
    }

    public async ValueTask<bool> HasAllPermissionsAsync(IReadOnlyCollection<string> permissions, CancellationToken ct = default)
    {
        if (permissions.Count == 0 || !currentUser.UserId.HasValue || !tenantContext.TenantId.HasValue)
        {
            return false;
        }

        var cached = await permissionCacheReader.GetPermissionsAsync(tenantContext.TenantId.Value, currentUser.UserId.Value, ct);
        return permissions.All(cached.Contains);
    }
}
