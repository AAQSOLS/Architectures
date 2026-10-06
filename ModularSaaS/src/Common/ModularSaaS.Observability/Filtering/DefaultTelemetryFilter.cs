using Microsoft.AspNetCore.Http;
using ModularSaaS.Observability.Abstractions;

namespace ModularSaaS.Observability.Filtering;

public sealed class DefaultTelemetryFilter : ITelemetryFilter
{
    private static readonly string[] ExcludedPrefixes =
    [
        "/health",
        "/swagger",
        "/openapi",
        "/scalar",
        "/favicon.ico"
    ];

    public bool ShouldExclude(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var path = context.Request.Path.Value;
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return ExcludedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
