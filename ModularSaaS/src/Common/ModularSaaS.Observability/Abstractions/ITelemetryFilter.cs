using Microsoft.AspNetCore.Http;

namespace ModularSaaS.Observability.Abstractions;

public interface ITelemetryFilter
{
    public bool ShouldExclude(HttpContext context);
}
