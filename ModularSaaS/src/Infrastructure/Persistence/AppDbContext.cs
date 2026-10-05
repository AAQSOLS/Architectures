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
    private static readonly MethodInfo SetQueryFilterMethod = typeof(AppDbContext)
        .GetMethod(nameof(SetQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

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
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var genericMethod = SetQueryFilterMethod.MakeGenericMethod(entityType.ClrType);
                genericMethod.Invoke(this, [modelBuilder]);
            }
        }
    }

    private void SetQueryFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            tenantContext.IsPlatformScope || e.TenantId == tenantContext.TenantId);
    }
}
