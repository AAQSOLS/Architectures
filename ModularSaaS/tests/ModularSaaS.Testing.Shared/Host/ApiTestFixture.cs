using ModularSaaS.Testing.Shared.Fixtures;
using Xunit;

namespace ModularSaaS.Testing.Shared.Host;

public sealed class ApiTestFixture : CustomWebApplicationFactory, IAsyncLifetime
{
    private static readonly PostgreSqlDatabaseFixture SharedDb = new();

    public ApiTestFixture() : base(SharedDb)
    {
    }

    public async Task InitializeAsync()
    {
        await SharedDb.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await SharedDb.DisposeAsync();
    }
}
