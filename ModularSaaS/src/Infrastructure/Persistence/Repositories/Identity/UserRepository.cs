using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Repositories.Identity;

internal sealed class UserRepository(AppDbContext dbContext, IClock clock)
    : EfRepository<User>(dbContext), IUserRepository
{
    public async Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await DbContext.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == normalizedEmail, ct);
    }

    public async Task<bool> EmailExistsAsync(Guid tenantId, string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await DbContext.Users
            .AnyAsync(u => u.TenantId == tenantId && u.Email == normalizedEmail, ct);
    }

    public async Task<IReadOnlyList<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        return await DbContext.UserRoles
            .Where(ur => ur.UserId == userId && (!ur.ExpiresAtUtc.HasValue || ur.ExpiresAtUtc.Value > now))
            .Select(ur => ur.Role.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        return await DbContext.UserRoles
            .Where(ur => ur.UserId == userId && (!ur.ExpiresAtUtc.HasValue || ur.ExpiresAtUtc.Value > now))
            .Select(ur => ur.Role)
            .ToListAsync(ct);
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId, Guid? assignedBy = null, DateTimeOffset? expiresAtUtc = null, CancellationToken ct = default)
    {
        var user = await DbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        user?.AssignRole(roleId, clock.UtcNow, assignedBy, expiresAtUtc);
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var user = await DbContext.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        user?.RemoveRole(roleId);
    }

    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var now = clock.UtcNow;

        // Permissions granted via active roles
        var rolePermissions = await DbContext.UserRoles
            .Where(ur => ur.UserId == userId && (!ur.ExpiresAtUtc.HasValue || ur.ExpiresAtUtc.Value > now))
            .SelectMany(ur => DbContext.RolePermissions.Where(rp => rp.RoleId == ur.RoleId).Select(rp => rp.Permission.Code))
            .ToListAsync(ct);

        // Direct user grants
        var directGrants = await DbContext.UserPermissions
            .Where(up => up.UserId == userId && up.IsGranted)
            .Select(up => up.Permission.Code)
            .ToListAsync(ct);

        // Direct user denies
        var directDenies = await DbContext.UserPermissions
            .Where(up => up.UserId == userId && !up.IsGranted)
            .Select(up => up.Permission.Code)
            .ToListAsync(ct);

        var effective = new HashSet<string>(rolePermissions, StringComparer.OrdinalIgnoreCase);
        foreach (var grant in directGrants)
        {
            effective.Add(grant);
        }

        foreach (var deny in directDenies)
        {
            effective.Remove(deny);
        }

        return effective;
    }

    public async Task SetDirectPermissionAsync(Guid userId, Guid permissionId, bool isGranted, CancellationToken ct = default)
    {
        var user = await DbContext.Users
            .Include(u => u.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        user?.SetDirectPermission(permissionId, isGranted);
    }

    public async Task<PagedResult<UserListItem>> ListUsersAsync(Guid tenantId, PageRequest page, CancellationToken ct = default)
    {
        var query = DbContext.Users.AsNoTracking().Where(u => u.TenantId == tenantId);
        var total = await query.CountAsync(ct);

        var now = clock.UtcNow;
        var users = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.Take)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Status,
                u.CreatedAtUtc,
                Roles = DbContext.UserRoles
                    .Where(ur => ur.UserId == u.Id && (!ur.ExpiresAtUtc.HasValue || ur.ExpiresAtUtc.Value > now))
                    .Select(ur => ur.Role.Name)
                    .ToList()
            })
            .ToListAsync(ct);

        var items = users.Select(u => new UserListItem(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.Status,
            u.Roles,
            u.CreatedAtUtc)).ToList();

        return new PagedResult<UserListItem>(items, total, page.PageNumber, page.PageSize);
    }
}
