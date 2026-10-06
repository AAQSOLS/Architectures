using ModularSaaS.Testing.Shared.Extensions;
using ModularSaaS.Testing.Shared.Fixtures;
using ModularSaaS.Testing.Shared.Host;
using Xunit;

namespace ModularSaaS.Functional.Tests.Common;

[Collection("Api")]
public abstract class FunctionalTestBase : IAsyncLifetime
{
    private readonly DatabaseResetter _resetter = new();

    protected ApiTestFixture Fixture { get; }

    protected FunctionalTestBase(ApiTestFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _resetter.ResetAsync(Fixture.ConnectionString);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected HttpClient CreateClientAsUser(
        Guid tenantId,
        Guid userId = default,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? roles = null)
    {
        var uid = userId == default ? Guid.NewGuid() : userId;
        var client = Fixture.CreateClient();
        return client.AsTenantUser(tenantId, uid, permissions, roles);
    }

    protected HttpClient CreateClientAsPlatformAdmin(Guid? userId = null)
    {
        var client = Fixture.CreateClient();
        return client.AsPlatformAdmin(userId);
    }

    protected HttpClient CreateAnonymousClient()
    {
        var client = Fixture.CreateClient();
        return client.AsAnonymous();
    }
}
