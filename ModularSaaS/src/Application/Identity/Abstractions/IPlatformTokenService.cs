namespace ModularSaaS.Application.Identity.Abstractions;

public interface IPlatformTokenService
{
    public string GeneratePlatformToken(Guid userId, string email, string fullName);

    public string GenerateImpersonationToken(Guid targetUserId, Guid targetTenantId, Guid actorId, string actorEmail, IEnumerable<string> roles, IEnumerable<string> permissions);
}
