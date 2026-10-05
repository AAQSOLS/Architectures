using ModularSaaS.Api.Common;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Middleware;

internal sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantSetter tenantSetter)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Platform endpoints operate in platform scope
        if (path.StartsWith(ApiRoutes.PlatformPrefix, StringComparison.OrdinalIgnoreCase))
        {
            tenantSetter.SetTenant(null, isPlatformScope: true);
            await next(context);
            return;
        }

        Guid? tenantId = null;

        // 1. Authenticated users: JWT claim ALWAYS dictates the tenant (cannot be spoofed)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claimValue = context.User.FindFirst(AppClaimTypes.TenantId)?.Value;
            if (Guid.TryParse(claimValue, out var claimTenantId))
            {
                tenantId = claimTenantId;
            }
        }
        // 2. Unauthenticated requests (e.g. login, public register): Resolve from X-Tenant-Id header
        else if (context.Request.Headers.TryGetValue(AppHeaders.TenantId, out var headerValue) &&
                 Guid.TryParse(headerValue.ToString(), out var headerTenantId))
        {
            tenantId = headerTenantId;
        }

        // 3. Resolve impersonation state from JWT claims
        var isImpersonated = string.Equals(context.User.FindFirst(AppClaimTypes.IsImpersonated)?.Value, AppClaimValues.True, StringComparison.OrdinalIgnoreCase);
        Guid? impersonatedBy = null;
        if (isImpersonated)
        {
            var act = context.User.FindFirst(AppClaimTypes.Actor)?.Value;
            if (Guid.TryParse(act, out var actorId))
            {
                impersonatedBy = actorId;
            }
        }

        tenantSetter.SetTenant(tenantId, isPlatformScope: false, isImpersonated: isImpersonated, impersonatedBy: impersonatedBy);
        await next(context);
    }
}
