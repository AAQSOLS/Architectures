namespace ModularSaaS.Application.Shared.Abstractions;

public interface IPermissionCacheInvalidator
{
    public Task InvalidateAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    public Task InvalidateRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default);
}
