using System.Security.Claims;
using ModularSaaS.Security.Constants;

namespace ModularSaaS.Security;

public static class CurrentUserExtensions
{
    public static bool IsAuthenticated(this ICurrentUser user) =>
        user.Principal?.Identity?.IsAuthenticated ?? false;

    public static string? GetEmail(this ICurrentUser user) =>
        user.FindClaim(SecurityClaimTypes.Email) ?? user.FindClaim(ClaimTypes.Email);

    public static string? GetName(this ICurrentUser user) =>
        user.FindClaim(SecurityClaimTypes.Name) ?? user.FindClaim(ClaimTypes.Name);

    public static bool HasRole(this ICurrentUser user, string role) =>
        user.Principal?.IsInRole(role) == true ||
        user.Principal?.HasClaim(c => (string.Equals(c.Type, SecurityClaimTypes.Role, StringComparison.Ordinal) ||
                                       string.Equals(c.Type, ClaimTypes.Role, StringComparison.Ordinal)) &&
                                      string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase)) == true;

    public static bool IsPlatformAdmin(this ICurrentUser user) =>
        user.HasRole(SecurityRoles.PlatformAdmin) ||
        string.Equals(user.FindClaim(SecurityClaimTypes.Scope), "Platform", StringComparison.OrdinalIgnoreCase);

    public static string? FindClaim(this ICurrentUser user, string claimType) =>
        user.Principal?.FindFirst(claimType)?.Value;

    public static IReadOnlyList<string> GetRoles(this ICurrentUser user) =>
        user.Principal?.FindAll(c => string.Equals(c.Type, SecurityClaimTypes.Role, StringComparison.Ordinal) ||
                                     string.Equals(c.Type, ClaimTypes.Role, StringComparison.Ordinal))
            .Select(c => c.Value)
            .Distinct(StringComparer.Ordinal)
            .ToList() ?? [];

    public static IReadOnlyList<string> GetPermissions(this ICurrentUser user) =>
        user.Principal?.FindAll(c => string.Equals(c.Type, SecurityClaimTypes.Permission, StringComparison.Ordinal))
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];
}
