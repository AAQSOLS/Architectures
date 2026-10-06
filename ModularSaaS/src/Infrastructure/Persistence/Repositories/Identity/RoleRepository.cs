using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Repositories.Identity;

internal sealed class RoleRepository(AppDbContext dbContext)
    : EfRepository<Role>(dbContext), IRoleRepository
{
    public async Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default)
    {
        return await DbContext.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == name.Trim(), ct);
    }

    public async Task<Role?> GetDefaultRoleAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await DbContext.Roles
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.IsDefault, ct);
    }

    public async Task<IReadOnlyList<Role>> ListAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await DbContext.Roles
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetRolePermissionCodesAsync(Guid roleId, CancellationToken ct = default)
    {
        return await DbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Code)
            .OrderBy(c => c)
            .ToListAsync(ct);
    }

    public async Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default)
    {
        var role = await DbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, ct);

        role?.SetPermissions(permissionIds);
    }
}
