using Microsoft.AspNetCore.Http;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.Abstractions.Constants;

namespace ModularSaaS.Security.AspNetCore.Identity;

public sealed class HttpImpersonationContext(IHttpContextAccessor httpContextAccessor) : IImpersonationContext
{
    private HttpContext? Context => httpContextAccessor.HttpContext;

    public bool IsImpersonated =>
        string.Equals(Context?.User.FindFirst(SecurityClaimTypes.IsImpersonated)?.Value, "true", StringComparison.OrdinalIgnoreCase);

    public Guid? ImpersonatorUserId
    {
        get
        {
            var act = Context?.User.FindFirst(SecurityClaimTypes.Actor)?.Value;
            return Guid.TryParse(act, out var id) ? id : null;
        }
    }

    public string? ImpersonatorEmail =>
        Context?.User.FindFirst(SecurityClaimTypes.ActorEmail)?.Value;
}
