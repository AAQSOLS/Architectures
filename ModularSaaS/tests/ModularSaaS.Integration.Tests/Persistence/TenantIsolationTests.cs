using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Integration.Tests.Common;
using ModularSaaS.Testing.Shared.Builders;
using ModularSaaS.Testing.Shared.Fixtures;
using Xunit;

namespace ModularSaaS.Integration.Tests.Persistence;

public class TenantIsolationTests : IntegrationTestBase
{
    public TenantIsolationTests(PostgreSqlDatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Query_WhenScopedToTenantA_NeverReturnsTenantBRecords()
    {
        var tenantA = new TenantBuilder().WithName("Tenant A").Build();
        var tenantB = new TenantBuilder().WithName("Tenant B").Build();

        await using (var seedContext = CreateDbContext(isPlatformScope: true))
        {
            seedContext.Tenants.AddRange(tenantA, tenantB);
            await seedContext.SaveChangesAsync();

            var userA = new UserBuilder().WithTenantId(tenantA.Id).WithEmail("usera@tenanta.com").Build();
            var userB = new UserBuilder().WithTenantId(tenantB.Id).WithEmail("userb@tenantb.com").Build();

            seedContext.Users.AddRange(userA, userB);
            await seedContext.SaveChangesAsync();
        }

        // Act: Query as Tenant A
        await using (var contextA = CreateDbContext(tenantA.Id))
        {
            var users = await contextA.Users.ToListAsync();

            // Assert
            users.Should().ContainSingle();
            users.Single().Email.Should().Be("usera@tenanta.com");
            users.Single().TenantId.Should().Be(tenantA.Id);
        }
    }

    [Fact]
    public async Task SaveChanges_WhenAddingTenantEntityWithoutExplicitTenantId_TenantInterceptorStampsTenantId()
    {
        var tenant = new TenantBuilder().Build();

        await using (var setupContext = CreateDbContext(isPlatformScope: true))
        {
            setupContext.Tenants.Add(tenant);
            await setupContext.SaveChangesAsync();
        }

        // Act: Add user with Guid.Empty as TenantId using Tenant-scoped context
        await using (var context = CreateDbContext(tenant.Id))
        {
            var user = new User(Guid.Empty, "autostamp@example.com", "hash", "Auto", "Stamp");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var verifyContext = CreateDbContext(tenant.Id))
        {
            var savedUser = await verifyContext.Users.FirstOrDefaultAsync(u => u.Email == "autostamp@example.com");
            savedUser.Should().NotBeNull();
            savedUser.TenantId.Should().Be(tenant.Id);
        }
    }
}
