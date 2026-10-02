using System.Security.Claims;

namespace ModularSaaS.Security.Abstractions;

public interface ITokenService
{
    public SecurityTokenResult CreateToken(TokenDescriptor descriptor);

    public ClaimsPrincipal? ValidateToken(string token);
}
