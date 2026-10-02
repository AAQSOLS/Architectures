using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Permissions;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Platform;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Infrastructure.Persistence.Seed;

internal sealed class DbInitializer(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IClock clock,
    IOptions<SeedOptions> options)
{
    private readonly SeedOptions _options = options.Value;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await SeedPermissionsAsync(ct);
        await SeedPlatformAdminAsync(ct);
        await SeedSystemTenantAsync(ct);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existingCodes = await dbContext.Permissions
            .Select(p => p.Code)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

        var permissionsToSeed = new List<Permission>();

        foreach (var code in AppPermissions.All)
        {
            if (existingCodes.Contains(code))
            {
                continue;
            }

            var parts = code.Split('.');
            var module = parts.Length > 0 ? parts[0] : "General";
            var resource = parts.Length > 1 ? parts[1] : string.Empty;
            var action = parts.Length > 2 ? parts[2] : string.Empty;
            var name = $"{action} {resource}".Trim();

            permissionsToSeed.Add(new Permission(code, name, module, $"Allows user to {action.ToLowerInvariant()} {resource.ToLowerInvariant()}."));
        }

        if (permissionsToSeed.Count > 0)
        {
            await dbContext.Permissions.AddRangeAsync(permissionsToSeed, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task SeedPlatformAdminAsync(CancellationToken ct)
    {
        var exists = await dbContext.PlatformUsers.AnyAsync(u => u.Email == _options.PlatformAdminEmail, ct);
        if (!exists)
        {
            var hash = passwordHasher.Hash(_options.PlatformAdminPassword);
            var superAdmin = new PlatformUser(_options.PlatformAdminEmail, hash, _options.PlatformAdminFirstName, _options.PlatformAdminLastName);
            await dbContext.PlatformUsers.AddAsync(superAdmin, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task SeedSystemTenantAsync(CancellationToken ct)
    {
        var tenantId = SeedOptions.DefaultTenantId;
        var tenantExists = await dbContext.Tenants.AnyAsync(t => t.Id == tenantId, ct);
        if (!tenantExists)
        {
            var tenant = new Tenant(tenantId, _options.DefaultTenantName, _options.DefaultTenantIdentifier, TenantPlan.Enterprise);
            await dbContext.Tenants.AddAsync(tenant, ct);
            await dbContext.SaveChangesAsync(ct);

            // Seed Admin and Member roles for the default tenant
            var adminRole = new Role(tenantId, AppRoles.Admin, "Tenant Administrator with full access", isSystem: true, isDefault: false);
            var memberRole = new Role(tenantId, AppRoles.Member, "Default tenant member", isSystem: false, isDefault: true);

            await dbContext.Roles.AddRangeAsync([adminRole, memberRole], ct);
            await dbContext.SaveChangesAsync(ct);

            // Assign all permissions to Admin role
            var allPermissions = await dbContext.Permissions.ToListAsync(ct);
            var rolePermissions = allPermissions.Select(p => new RolePermission(adminRole.Id, p.Id));
            await dbContext.RolePermissions.AddRangeAsync(rolePermissions, ct);

            // Seed initial tenant administrator
            var adminHash = passwordHasher.Hash(_options.DefaultTenantAdminPassword);
            var adminUser = new User(tenantId, _options.DefaultTenantAdminEmail, adminHash, _options.DefaultTenantAdminFirstName, _options.DefaultTenantAdminLastName);
            adminUser.ConfirmEmail();
            await dbContext.Users.AddAsync(adminUser, ct);
            await dbContext.SaveChangesAsync(ct);

            var userRole = new UserRole(adminUser.Id, adminRole.Id, clock.UtcNow);
            await dbContext.UserRoles.AddAsync(userRole, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
