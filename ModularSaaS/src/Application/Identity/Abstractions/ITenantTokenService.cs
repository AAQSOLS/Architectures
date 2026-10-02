namespace ModularSaaS.Application.Identity.Abstractions;

public interface ITenantTokenService
{
    public string GenerateAccessToken(Guid userId, string email, Guid tenantId, IEnumerable<string> roles, IEnumerable<string> permissions);
}
