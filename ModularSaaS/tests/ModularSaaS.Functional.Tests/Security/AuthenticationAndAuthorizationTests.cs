using System.Net;
using FluentAssertions;
using ModularSaaS.Application.Identity.Permissions;
using ModularSaaS.Functional.Tests.Common;
using ModularSaaS.Testing.Shared.Host;
using Xunit;

namespace ModularSaaS.Functional.Tests.Security;

public class AuthenticationAndAuthorizationTests : FunctionalTestBase
{
    public AuthenticationAndAuthorizationTests(ApiTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Request_WhenAnonymous_Returns401Unauthorized()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WhenMissingRequiredPermission_Returns403Forbidden()
    {
        var tenantId = Guid.NewGuid();
        // Client without Identity.Users.Read permission
        var client = CreateClientAsUser(
            tenantId: tenantId,
            permissions: [AppPermissions.Tenancy.SettingsRead]);

        var response = await client.GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
