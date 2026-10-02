namespace ModularSaaS.Application.Identity.Abstractions;

public interface IUserPermissionReader
{
    public Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
}
