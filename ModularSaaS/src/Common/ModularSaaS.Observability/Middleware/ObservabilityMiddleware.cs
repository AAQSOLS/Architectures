using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Observability.Abstractions;
using ModularSaaS.Observability.Diagnostics;

namespace ModularSaaS.Observability.Middleware;

public sealed class ObservabilityMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var filter = context.RequestServices.GetService<ITelemetryFilter>();
        if (filter?.ShouldExclude(context) == true)
        {
            await next(context);
            return;
        }

        var activity = Activity.Current;
        if (activity is not null)
        {
            var enrichers = context.RequestServices.GetServices<ITelemetryEnricher>();
            foreach (var enricher in enrichers)
            {
                enricher.Enrich(activity, context);
            }
        }

        try
        {
            await next(context);
        }
        finally
        {
            var tenantId = activity?.GetTagItem("tenant.id")?.ToString();
            var route = context.GetEndpoint()?.DisplayName ?? context.Request.Path.Value;
            SaaSMetrics.RecordRequest(tenantId, context.Response.StatusCode, route);
        }
    }
}
