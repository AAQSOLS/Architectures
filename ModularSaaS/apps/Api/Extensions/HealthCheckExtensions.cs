using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Extensions;

public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions DetailedJsonOptions = new()
    {
        WriteIndented = true
    };

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API process is running."), tags: [HealthCheckTags.Live]);

        return services;
    }

    public static WebApplication MapAppHealthChecks(this WebApplication app)
    {
        // 1. Liveness: Process responsiveness only (no external dependencies)
        app.MapHealthChecks(HealthCheckEndpoints.Live, new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(HealthCheckTags.Live)
        });

        // 2. Readiness: External dependencies (Database, Redis, Storage)
        app.MapHealthChecks(HealthCheckEndpoints.Ready, new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(HealthCheckTags.Ready)
        });

        // 3. Fallback standard probe
        app.MapHealthChecks(HealthCheckEndpoints.Standard, new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(HealthCheckTags.Ready)
        });

        // 4. Detailed diagnostic probe for internal monitoring and admin dashboards
        app.MapHealthChecks(HealthCheckEndpoints.Details, new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    status = report.Status.ToString(),
                    totalDurationMs = report.TotalDuration.TotalMilliseconds,
                    entries = report.Entries.ToDictionary(
                        e => e.Key,
                        e => new
                        {
                            status = e.Value.Status.ToString(),
                            durationMs = e.Value.Duration.TotalMilliseconds,
                            description = e.Value.Description,
                            error = e.Value.Exception?.Message
                        },
                        StringComparer.Ordinal)
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, DetailedJsonOptions));
            }
        });

        return app;
    }
}
