using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Infrastructure.Persistence;
using ModularSaaS.Infrastructure.Persistence.Interceptors;
using ModularSaaS.Infrastructure.Time;
using ModularSaaS.Testing.Shared.Fixtures;
using ModularSaaS.Testing.Shared.Host.Fakes;
using Xunit;

namespace ModularSaaS.Integration.Tests.Common;

[Collection("Database")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected PostgreSqlDatabaseFixture DatabaseFixture { get; }
    private readonly DatabaseResetter _resetter = new();

    protected IntegrationTestBase(PostgreSqlDatabaseFixture databaseFixture)
    {
        DatabaseFixture = databaseFixture;
    }

    public async Task InitializeAsync()
    {
        await _resetter.ResetAsync(DatabaseFixture.ConnectionString);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    internal AppDbContext CreateDbContext(
        Guid? tenantId = null,
        bool isPlatformScope = false,
        Guid? currentUserId = null)
    {
        var tenantContext = new TestTenantContext(tenantId, isPlatformScope);
        var currentUser = new TestCurrentUser(currentUserId, isAuthenticated: currentUserId.HasValue);
        var clock = new SystemClock();

        var tenantInterceptor = new TenantInterceptor(tenantContext);
        var softDeleteInterceptor = new SoftDeleteInterceptor(clock, currentUser);
        var auditInterceptor = new AuditInterceptor(clock, currentUser);
        var outboxInterceptor = new OutboxInterceptor();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(DatabaseFixture.ConnectionString)
            .AddInterceptors(tenantInterceptor, softDeleteInterceptor, auditInterceptor, outboxInterceptor)
            .Options;

        return new AppDbContext(options, tenantContext);
    }
}
