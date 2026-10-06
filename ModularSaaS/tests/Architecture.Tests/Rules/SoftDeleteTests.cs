using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Platform;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Infrastructure.Persistence;
using ModularSaaS.Infrastructure.Persistence.Interceptors;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class SoftDeleteTests
{
    [Fact]
    public void AuditableEntity_Implements_ISoftDeletable()
    {
        Assert.True(typeof(ISoftDeletable).IsAssignableFrom(typeof(AuditableEntity)));
    }

    [Theory]
    [InlineData(typeof(User))]
    [InlineData(typeof(Role))]
    [InlineData(typeof(Tenant))]
    [InlineData(typeof(PlatformUser))]
    public void DomainEntities_Implement_ISoftDeletable(Type entityType)
    {
        Assert.True(
            typeof(ISoftDeletable).IsAssignableFrom(entityType),
            $"Entity '{entityType.Name}' must implement ISoftDeletable.");
    }

    [Fact]
    public void SoftDelete_And_Restore_Methods_Update_State_Correctly()
    {
        var tenant = new Tenant("Acme Corp", "acme-corp");
        var deletedBy = Guid.NewGuid();
        var deletedAt = DateTimeOffset.UtcNow;

        Assert.False(tenant.IsDeleted);
        Assert.Null(tenant.DeletedAtUtc);
        Assert.Null(tenant.DeletedBy);

        tenant.SoftDelete(deletedBy, deletedAt);

        Assert.True(tenant.IsDeleted);
        Assert.Equal(deletedAt, tenant.DeletedAtUtc);
        Assert.Equal(deletedBy, tenant.DeletedBy);

        tenant.Restore();

        Assert.False(tenant.IsDeleted);
        Assert.Null(tenant.DeletedAtUtc);
        Assert.Null(tenant.DeletedBy);
    }

    [Fact]
    public void AppDbContext_Model_Configures_QueryFilter_For_SoftDeletable_Entities()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=TestDb;Username=postgres;Password=postgres")
            .Options;

        var tenantContext = new TestTenantContext(Guid.NewGuid());
        using var dbContext = new AppDbContext(options, tenantContext);

        var model = dbContext.Model;

        var softDeletableTypes = model.GetEntityTypes()
            .Where(e => typeof(ISoftDeletable).IsAssignableFrom(e.ClrType) && e.BaseType is null);

        Assert.NotEmpty(softDeletableTypes);

        foreach (var entityType in softDeletableTypes)
        {
            var filters = entityType.GetDeclaredQueryFilters();
            Assert.NotEmpty(filters);
            var combinedFilterString = string.Join(" ", filters.Select(f => f.Expression?.ToString() ?? string.Empty));
            Assert.Contains("IsDeleted", combinedFilterString, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void SoftDeleteInterceptor_Converts_DeletedState_To_Modified_With_Audit_Info()
    {
        var testClock = new TestClock(new DateTimeOffset(2026, 10, 6, 12, 0, 0, TimeSpan.Zero));
        var testUserId = Guid.NewGuid();
        var testUser = new TestCurrentUser(testUserId);
        var interceptor = new SoftDeleteInterceptor(testClock, testUser);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=TestDb;Username=postgres;Password=postgres")
            .Options;

        var tenantContext = new TestTenantContext(Guid.NewGuid());
        using var dbContext = new AppDbContext(options, tenantContext);

        var user = new User(tenantContext.TenantId!.Value, "test@test.com", "hash", "Test", "User");
        dbContext.Attach(user);

        // Mark as deleted in change tracker
        dbContext.Entry(user).State = EntityState.Deleted;

        // Apply interceptor logic
        interceptor.ApplySoftDelete(dbContext);

        var entry = dbContext.Entry(user);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(user.IsDeleted);
        Assert.Equal(testClock.UtcNow, user.DeletedAtUtc);
        Assert.Equal(testUserId, user.DeletedBy);
    }

    private sealed class TestClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class TestCurrentUser(Guid userId) : ICurrentUser
    {
        public Guid? UserId => userId;
        public string? Email => "test@test.com";
        public bool IsAuthenticated => true;
        public bool IsPlatformAdmin => false;
        public bool IsImpersonated => false;
        public Guid? ActorId => null;
        public IReadOnlyList<string> Roles => [];
    }

    private sealed class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;
        public bool IsPlatformScope => false;
        public bool IsImpersonated => false;
        public Guid? ImpersonatedBy => null;
        public Guid RequireTenantId() => tenantId;
    }
}
