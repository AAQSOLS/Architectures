using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using ModularSaaS.Observability.Diagnostics;
using ModularSaaS.Observability.Filtering;
using ModularSaaS.Observability.Redaction;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class ObservabilityTests
{
    [Fact]
    public void SaaSActivities_Starts_Activity_When_Observed()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == SaaSActivities.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = SaaSActivities.StartActivity("TestOperation");

        Assert.NotNull(activity);
        Assert.Equal(SaaSActivities.SourceName, activity.Source.Name);
        Assert.Equal("TestOperation", activity.OperationName);
    }

    [Fact]
    public void SensitiveDataSanitizer_Masks_QueryStrings()
    {
        var rawUrl = "/api/auth/reset-password?token=SuperSecretToken12345&email=user@test.com";
        var sanitized = SensitiveDataSanitizer.SanitizeUrl(rawUrl);

        Assert.Equal("/api/auth/reset-password?REDACTED", sanitized);
        Assert.DoesNotContain("SuperSecretToken12345", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("user@test.com", sanitized, StringComparison.Ordinal);
    }

    [Fact]
    public void SensitiveDataSanitizer_Leaves_Clean_Urls_Intact()
    {
        var cleanUrl = "/api/v1/tenants/active";
        var sanitized = SensitiveDataSanitizer.SanitizeUrl(cleanUrl);

        Assert.Equal(cleanUrl, sanitized);
    }

    [Fact]
    public void SensitiveDataSanitizer_Detects_Sensitive_Headers()
    {
        Assert.True(SensitiveDataSanitizer.IsSensitiveHeader("Authorization"));
        Assert.True(SensitiveDataSanitizer.IsSensitiveHeader("authorization"));
        Assert.True(SensitiveDataSanitizer.IsSensitiveHeader("Cookie"));
        Assert.True(SensitiveDataSanitizer.IsSensitiveHeader("X-Api-Key"));

        Assert.False(SensitiveDataSanitizer.IsSensitiveHeader("Content-Type"));
        Assert.False(SensitiveDataSanitizer.IsSensitiveHeader("Accept"));
        Assert.False(SensitiveDataSanitizer.IsSensitiveHeader("Host"));
    }

    [Fact]
    public void DefaultTelemetryFilter_Excludes_Health_And_Swagger()
    {
        var filter = new DefaultTelemetryFilter();

        var healthContext = CreateHttpContext("/health");
        var healthReadyContext = CreateHttpContext("/health/ready");
        var swaggerContext = CreateHttpContext("/swagger/v1/swagger.json");
        var faviconContext = CreateHttpContext("/favicon.ico");
        var apiContext = CreateHttpContext("/api/users/current");

        Assert.True(filter.ShouldExclude(healthContext));
        Assert.True(filter.ShouldExclude(healthReadyContext));
        Assert.True(filter.ShouldExclude(swaggerContext));
        Assert.True(filter.ShouldExclude(faviconContext));

        Assert.False(filter.ShouldExclude(apiContext));
    }

    [Fact]
    public void SaaSMetrics_Records_Without_Exceptions()
    {
        var exception = Record.Exception(() =>
        {
            SaaSMetrics.RecordRequest("tenant-123", 200, "/api/users");
            SaaSMetrics.RecordLogin(isSuccess: true);
            SaaSMetrics.RecordLogin(isSuccess: false, failureReason: "BadPassword");
            SaaSMetrics.RecordTokenRevocation();
            SaaSMetrics.RecordTenantCreated();
        });

        Assert.Null(exception);
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return context;
    }
}
