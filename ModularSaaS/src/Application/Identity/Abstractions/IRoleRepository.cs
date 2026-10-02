using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IRoleRepository : IRepository<Role>
{
    public Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default);

    public Task<Role?> GetDefaultRoleAsync(Guid tenantId, CancellationToken ct = default);

    public Task<IReadOnlyList<Role>> ListAsync(Guid tenantId, CancellationToken ct = default);

    public Task<IReadOnlyList<string>> GetRolePermissionCodesAsync(Guid roleId, CancellationToken ct = default);

    public Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default);
}
