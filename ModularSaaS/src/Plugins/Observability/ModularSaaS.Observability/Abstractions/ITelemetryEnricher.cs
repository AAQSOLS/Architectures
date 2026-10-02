using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace ModularSaaS.Observability.Abstractions;

public interface ITelemetryEnricher
{
    public void Enrich(Activity activity, HttpContext context);
}
