using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Platform;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Infrastructure.Persistence.Outbox;

namespace ModularSaaS.Infrastructure.Persistence;

internal class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ITenantContext tenantContext) : DbContext(options)
{
    private static readonly MethodInfo SetTenantAndSoftDeleteQueryFilterMethod = typeof(AppDbContext)
        .GetMethod(nameof(SetTenantAndSoftDeleteQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo SetTenantOnlyQueryFilterMethod = typeof(AppDbContext)
        .GetMethod(nameof(SetTenantOnlyQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo SetSoftDeleteOnlyQueryFilterMethod = typeof(AppDbContext)
        .GetMethod(nameof(SetSoftDeleteOnlyQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.BaseType is not null || entityType.IsOwned())
            {
                continue;
            }

            var clrType = entityType.ClrType;
            var isTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);
            var isSoftDelete = typeof(ISoftDeletable).IsAssignableFrom(clrType);

            if (isTenant && isSoftDelete)
            {
                SetTenantAndSoftDeleteQueryFilterMethod.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
            }
            else if (isTenant)
            {
                SetTenantOnlyQueryFilterMethod.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
            }
            else if (isSoftDelete)
            {
                SetSoftDeleteOnlyQueryFilterMethod.MakeGenericMethod(clrType).Invoke(null, [modelBuilder]);
            }
        }
    }

    private void SetTenantAndSoftDeleteQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !e.IsDeleted && (tenantContext.IsPlatformScope || e.TenantId == tenantContext.TenantId));
    }

    private void SetTenantOnlyQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            tenantContext.IsPlatformScope || e.TenantId == tenantContext.TenantId);
    }

    private static void SetSoftDeleteOnlyQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }
}
