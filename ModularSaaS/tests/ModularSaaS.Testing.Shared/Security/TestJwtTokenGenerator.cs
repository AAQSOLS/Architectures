using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Testing.Shared.Security;

public static class TestJwtTokenGenerator
{
    public const string DefaultSecret = "DevelopmentOnlyLocalSigningKey_MustBeReplacedInProductionMin32Bytes!";
    public const string DefaultIssuer = "ModularSaaS";
    public const string DefaultAudience = "ModularSaaS.Client";

    public static string GenerateUserToken(
        Guid tenantId,
        Guid userId,
        string email = "testuser@example.com",
        IEnumerable<string>? roles = null,
        IEnumerable<string>? permissions = null,
        string secret = DefaultSecret,
        string issuer = DefaultIssuer,
        string audience = DefaultAudience)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppClaimTypes.TenantId, tenantId.ToString())
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(AppClaimTypes.Permission, permission));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GeneratePlatformToken(
        Guid userId,
        string email = "platformadmin@example.com",
        string secret = DefaultSecret,
        string issuer = DefaultIssuer,
        string audience = DefaultAudience)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppClaimTypes.Scope, AppClaimValues.Platform),
            new(ClaimTypes.Role, AppRoles.PlatformAdmin)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
