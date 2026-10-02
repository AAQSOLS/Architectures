using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity;

public class PasswordResetToken : BaseEntity, ITenantEntity
{
    private PasswordResetToken()
    {
    }

    public PasswordResetToken(Guid tenantId, Guid userId, string tokenHash, DateTimeOffset expiresAtUtc, DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        TenantId = tenantId;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? UsedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public bool IsValid(DateTimeOffset now) => UsedAtUtc is null && ExpiresAtUtc > now;

    public void Use(DateTimeOffset usedAtUtc)
    {
        UsedAtUtc = usedAtUtc;
    }
}
