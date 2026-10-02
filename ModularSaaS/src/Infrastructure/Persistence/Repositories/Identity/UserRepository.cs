using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Shared.Abstractions;
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
        var existing = await DbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

        if (existing is null)
        {
            var userRole = new UserRole(userId, roleId, clock.UtcNow, assignedBy, expiresAtUtc);
            await DbContext.UserRoles.AddAsync(userRole, ct);
        }
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var existing = await DbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

        if (existing is not null)
        {
            DbContext.UserRoles.Remove(existing);
        }
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
        var existing = await DbContext.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId, ct);

        if (existing is null)
        {
            var up = new UserPermission(userId, permissionId, isGranted);
            await DbContext.UserPermissions.AddAsync(up, ct);
        }
        else
        {
            existing.SetGranted(isGranted);
        }
    }
}
