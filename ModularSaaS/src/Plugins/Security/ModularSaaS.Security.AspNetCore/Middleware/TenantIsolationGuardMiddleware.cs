using Microsoft.AspNetCore.Http;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.Abstractions.Extensions;

namespace ModularSaaS.Security.AspNetCore.Middleware;

public sealed class TenantIsolationGuardMiddleware(RequestDelegate next)
{
    public const string DefaultTenantHeader = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated() && !currentUser.IsPlatformAdmin())
        {
            var tokenTenantId = currentUser.TenantId;

            if (tokenTenantId.HasValue &&
                context.Request.Headers.TryGetValue(DefaultTenantHeader, out var headerVal) &&
                Guid.TryParse(headerVal.ToString(), out var headerTenantId))
            {
                if (tokenTenantId.Value != headerTenantId)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        type = "https://httpstatuses.com/403",
                        title = "TenantMismatch",
                        status = StatusCodes.Status403Forbidden,
                        detail = "Security token tenant does not match the requested tenant context."
                    });
                    return;
                }
            }
        }

        await next(context);
    }
}
