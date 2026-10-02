using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Infrastructure.Security;

internal sealed class JwtTokenService(IOptions<JwtOptions> options, IClock clock) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateAccessToken(Guid userId, string email, Guid tenantId, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppClaimTypes.TenantId, tenantId.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(AppClaimTypes.Permission, permission));
        }

        return CreateToken(claims, TimeSpan.FromMinutes(_options.ExpiryMinutes));
    }

    public string GeneratePlatformToken(Guid userId, string email, string fullName)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppClaimTypes.Scope, AppClaimValues.Platform),
            new(ClaimTypes.Role, AppRoles.PlatformAdmin)
        };

        return CreateToken(claims, TimeSpan.FromHours(SecurityPolicies.PlatformTokenLifetimeHours));
    }

    public string GenerateImpersonationToken(Guid targetUserId, Guid targetTenantId, Guid actorId, string actorEmail, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, targetUserId.ToString()),
            new(AppClaimTypes.TenantId, targetTenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(AppClaimTypes.IsImpersonated, AppClaimValues.True),
            new(AppClaimTypes.Actor, actorId.ToString()),
            new(AppClaimTypes.ActorEmail, actorEmail)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim(AppClaimTypes.Permission, permission));
        }

        return CreateToken(claims, TimeSpan.FromMinutes(SecurityPolicies.ImpersonationTokenLifetimeMinutes));
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(SecurityPolicies.RefreshTokenByteCount));
    }

    public string GeneratePasswordResetToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(SecurityPolicies.PasswordResetTokenByteCount));
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private string CreateToken(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = clock.UtcNow.UtcDateTime;
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(lifetime),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
