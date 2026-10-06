using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModularSaaS.Domain.Identity;
using ModularSaaS.Integration.Tests.Common;
using ModularSaaS.Testing.Shared.Builders;
using ModularSaaS.Testing.Shared.Fixtures;
using Xunit;

namespace ModularSaaS.Integration.Tests.Persistence;

public class OutboxInterceptorTests : IntegrationTestBase
{
    public OutboxInterceptorTests(MsSqlDatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task SaveChanges_WhenEntityRaisesDomainEvent_PersistsOutboxMessageInSameTransaction()
    {
        var tenant = new TenantBuilder().Build();

        await using (var seedContext = CreateDbContext(isPlatformScope: true))
        {
            seedContext.Tenants.Add(tenant);
            await seedContext.SaveChangesAsync();
        }

        // Act: Create User (User constructor emits UserCreatedDomainEvent)
        await using (var context = CreateDbContext(tenant.Id))
        {
            var user = new User(tenant.Id, "eventuser@example.com", "hash", "Event", "User");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Assert: Verify OutboxMessage was saved
        await using (var verifyContext = CreateDbContext(isPlatformScope: true))
        {
            var outboxMessages = await verifyContext.OutboxMessages.ToListAsync();
            outboxMessages.Should().Contain(m =>
                m.Type.Contains("UserCreatedDomainEvent") &&
                m.Content.Contains("eventuser@example.com") &&
                m.ProcessedOnUtc == null);
        }
    }
}
