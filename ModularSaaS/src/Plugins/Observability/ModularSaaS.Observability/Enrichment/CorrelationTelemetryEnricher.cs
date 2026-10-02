using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using ModularSaaS.Observability.Abstractions;

namespace ModularSaaS.Observability.Enrichment;

public sealed class CorrelationTelemetryEnricher : ITelemetryEnricher
{
    public const string CorrelationHeader = "X-Correlation-Id";

    public void Enrich(Activity activity, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId) ||
            string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = activity.TraceId.ToString();
            context.Response.Headers[CorrelationHeader] = correlationId;
        }

        activity.SetTag("correlation.id", correlationId.ToString());
    }
}
