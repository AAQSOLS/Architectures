using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularSaaS.Observability.Abstractions;
using ModularSaaS.Observability.Diagnostics;
using ModularSaaS.Observability.Enrichment;
using ModularSaaS.Observability.Filtering;
using ModularSaaS.Observability.Middleware;
using ModularSaaS.Observability.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ModularSaaS.Observability;

public static class DependencyInjection
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ObservabilityOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new ObservabilityOptions();
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);
        configure?.Invoke(options);

        services.Configure<ObservabilityOptions>(configuration.GetSection(ObservabilityOptions.SectionName));

        // 1. Pluggable filters and enrichers
        services.TryAddSingleton<ITelemetryFilter, DefaultTelemetryFilter>();
        services.AddSingleton<ITelemetryEnricher, TenantTelemetryEnricher>();
        services.AddSingleton<ITelemetryEnricher, CorrelationTelemetryEnricher>();

        // 2. OpenTelemetry Tracing & Metrics
        if (!string.Equals(options.Exporter, "None", StringComparison.OrdinalIgnoreCase))
        {
            var otelBuilder = services.AddOpenTelemetry();

            otelBuilder.ConfigureResource(resource => resource
                .AddService(options.ServiceName, serviceVersion: options.ServiceVersion));

            otelBuilder.WithTracing(tracing =>
            {
                tracing.AddSource(SaaSActivities.SourceName);
                tracing.AddAspNetCoreInstrumentation(o =>
                {
                    o.RecordException = true;
                });
                tracing.AddHttpClientInstrumentation();

                if (options.Sampling.DefaultRatio < 1.0)
                {
                    tracing.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(options.Sampling.DefaultRatio)));
                }

                if (string.Equals(options.Exporter, "Otlp", StringComparison.OrdinalIgnoreCase))
                {
                    tracing.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(options.Otlp.Endpoint);
                    });
                }
                else if (string.Equals(options.Exporter, "Console", StringComparison.OrdinalIgnoreCase))
                {
                    tracing.AddConsoleExporter();
                }
            });

            otelBuilder.WithMetrics(metrics =>
            {
                metrics.AddMeter(SaaSMetrics.MeterName);
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddRuntimeInstrumentation();

                if (string.Equals(options.Exporter, "Otlp", StringComparison.OrdinalIgnoreCase))
                {
                    metrics.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(options.Otlp.Endpoint);
                    });
                }
                else if (string.Equals(options.Exporter, "Console", StringComparison.OrdinalIgnoreCase))
                {
                    metrics.AddConsoleExporter();
                }
            });
        }

        return services;
    }

    public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<ObservabilityMiddleware>();

        return app;
    }
}
