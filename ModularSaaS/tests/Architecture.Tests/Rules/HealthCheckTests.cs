using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularSaaS.Api.Extensions;
using ModularSaaS.Application.Shared.Constants;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class HealthCheckTests
{
    [Fact]
    public async Task SelfCheck_Registers_Under_Live_Tag_And_Returns_Healthy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAppHealthChecks();

        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetRequiredService<HealthCheckService>();

        var report = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains(HealthCheckTags.Live));

        Assert.Equal(HealthStatus.Healthy, report.Status);
        Assert.True(report.Entries.ContainsKey("self"));
        Assert.Equal(HealthStatus.Healthy, report.Entries["self"].Status);
    }
}
