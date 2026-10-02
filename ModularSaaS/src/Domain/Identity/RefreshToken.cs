using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity;

public class RefreshToken : BaseEntity, ITenantEntity
{
    private RefreshToken()
    {
    }

    public RefreshToken(Guid tenantId, Guid userId, string token, DateTimeOffset expiresAtUtc, DateTimeOffset createdAtUtc, string? createdByIp = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        TenantId = tenantId;
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        CreatedByIp = createdByIp;
    }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? ReplacedByToken { get; private set; }

    public string? CreatedByIp { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public bool IsActive(DateTimeOffset now) => RevokedAtUtc is null && ExpiresAtUtc > now;

    public void Revoke(DateTimeOffset revokedAtUtc, string? replacedByToken = null)
    {
        RevokedAtUtc = revokedAtUtc;
        ReplacedByToken = replacedByToken;
    }
}
