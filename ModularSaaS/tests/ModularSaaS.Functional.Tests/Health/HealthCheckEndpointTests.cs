using System.Net;
using FluentAssertions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Functional.Tests.Common;
using ModularSaaS.Testing.Shared.Host;
using Xunit;

namespace ModularSaaS.Functional.Tests.Health;

public class HealthCheckEndpointTests : FunctionalTestBase
{
    public HealthCheckEndpointTests(ApiTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task LivenessCheck_ReturnsOk()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(HealthCheckEndpoints.Live);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
