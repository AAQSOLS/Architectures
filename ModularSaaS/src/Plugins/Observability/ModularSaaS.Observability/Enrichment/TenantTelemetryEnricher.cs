using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Observability.Abstractions;
using ModularSaaS.Security.Abstractions;

namespace ModularSaaS.Observability.Enrichment;

public sealed class TenantTelemetryEnricher : ITelemetryEnricher
{
    public void Enrich(Activity activity, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(context);

        var currentUser = context.RequestServices.GetService<ICurrentUser>();
        if (currentUser is null)
        {
            return;
        }

        if (currentUser.TenantId.HasValue)
        {
            activity.SetTag("tenant.id", currentUser.TenantId.Value.ToString());
        }

        if (currentUser.UserId.HasValue)
        {
            activity.SetTag("user.id", currentUser.UserId.Value.ToString());
        }
    }
}
