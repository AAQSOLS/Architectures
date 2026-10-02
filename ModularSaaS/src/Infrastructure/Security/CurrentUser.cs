using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Infrastructure.Security;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsPlatformAdmin =>
        User?.IsInRole(AppRoles.PlatformAdmin) == true ||
        string.Equals(User?.FindFirstValue(AppClaimTypes.Scope), AppClaimValues.Platform, StringComparison.OrdinalIgnoreCase);

    public bool IsImpersonated =>
        string.Equals(User?.FindFirstValue(AppClaimTypes.IsImpersonated), AppClaimValues.True, StringComparison.OrdinalIgnoreCase);

    public Guid? ActorId
    {
        get
        {
            var act = User?.FindFirstValue(AppClaimTypes.Actor);
            return Guid.TryParse(act, out var id) ? id : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
}
