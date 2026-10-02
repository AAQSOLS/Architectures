namespace ModularSaaS.Application.Identity.Abstractions;

public interface ITokenService : ITenantTokenService, IPlatformTokenService, ISecureTokenGenerator
{
}
