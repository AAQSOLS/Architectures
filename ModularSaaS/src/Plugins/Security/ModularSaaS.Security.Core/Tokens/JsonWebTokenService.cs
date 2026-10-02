using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.Abstractions.Constants;

namespace ModularSaaS.Security.Core.Tokens;

public sealed class JsonWebTokenService : ITokenService
{
    private readonly JwtServiceOptions _options;
    private readonly JsonWebTokenHandler _handler;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;
    private readonly TimeProvider _timeProvider;

    public JsonWebTokenService(JwtServiceOptions options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.SigningKey);

        if (Encoding.UTF8.GetByteCount(options.SigningKey) < 32)
        {
            throw new ArgumentException("JWT SigningKey must be at least 256 bits (32 bytes).", nameof(options));
        }

        _options = options;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _handler = new JsonWebTokenHandler
        {
            MapInboundClaims = false
        };

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var validationKeys = new List<SecurityKey> { _signingKey };
        foreach (var key in options.PreviousValidationKeys)
        {
            if (!string.IsNullOrWhiteSpace(key) && Encoding.UTF8.GetByteCount(key) >= 32)
            {
                validationKeys.Add(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)));
            }
        }

        _validationParameters = new TokenValidationParameters
        {
            ValidIssuer = options.Issuer,
            ValidateIssuer = !string.IsNullOrWhiteSpace(options.Issuer),
            ValidAudience = options.Audience,
            ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
            IssuerSigningKeys = validationKeys,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = options.ClockSkew,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256, SecurityAlgorithms.HmacSha512]
        };
    }

    public SecurityTokenResult CreateToken(TokenDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var now = _timeProvider.GetUtcNow();
        var lifetime = descriptor.Lifetime ?? _options.DefaultLifetime;
        var expires = now.Add(lifetime);
        var jti = Guid.NewGuid().ToString("N");

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Jti] = jti
        };

        if (descriptor.UserId.HasValue)
        {
            claims[JwtRegisteredClaimNames.Sub] = descriptor.UserId.Value.ToString();
        }

        if (descriptor.TenantId.HasValue)
        {
            claims[SecurityClaimTypes.TenantId] = descriptor.TenantId.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(descriptor.Email))
        {
            claims[JwtRegisteredClaimNames.Email] = descriptor.Email;
        }

        if (!string.IsNullOrWhiteSpace(descriptor.Name))
        {
            claims[JwtRegisteredClaimNames.Name] = descriptor.Name;
        }

        if (descriptor.Roles is not null)
        {
            var rolesList = descriptor.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
            if (rolesList.Count > 0)
            {
                claims[SecurityClaimTypes.Role] = rolesList;
            }
        }

        if (descriptor.Permissions is not null)
        {
            var permList = descriptor.Permissions.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (permList.Count > 0)
            {
                claims[SecurityClaimTypes.Permission] = permList;
            }
        }

        if (descriptor.ImpersonatorUserId.HasValue)
        {
            claims[SecurityClaimTypes.IsImpersonated] = "true";
            claims[SecurityClaimTypes.Actor] = descriptor.ImpersonatorUserId.Value.ToString();
            if (!string.IsNullOrWhiteSpace(descriptor.ImpersonatorEmail))
            {
                claims[SecurityClaimTypes.ActorEmail] = descriptor.ImpersonatorEmail;
            }
        }

        if (descriptor.AdditionalClaims is not null)
        {
            foreach (var claim in descriptor.AdditionalClaims)
            {
                claims[claim.Type] = claim.Value;
            }
        }

        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Claims = claims,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = _signingCredentials
        };

        var tokenString = _handler.CreateToken(tokenDescriptor);

        return new SecurityTokenResult(tokenString, jti, expires);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var result = _handler.ValidateTokenAsync(token, _validationParameters).GetAwaiter().GetResult();
        return result.IsValid ? new ClaimsPrincipal(result.ClaimsIdentity) : null;
    }
}
