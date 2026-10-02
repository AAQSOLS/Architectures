using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.Abstractions.Constants;

namespace ModularSaaS.Security.AspNetCore.Identity;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirst(SecurityClaimTypes.Subject)?.Value
                ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var tid = Principal?.FindFirst(SecurityClaimTypes.TenantId)?.Value;
            return Guid.TryParse(tid, out var id) ? id : null;
        }
    }
}
