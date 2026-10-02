namespace ModularSaaS.Application.Shared.Abstractions;

public interface IPermissionCacheReader
{
    public Task<IReadOnlySet<string>> GetPermissionsAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
}
