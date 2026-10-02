namespace ModularSaaS.Domain.Identity;

public class UserRole
{
    private UserRole()
    {
    }

    public UserRole(Guid userId, Guid roleId, DateTimeOffset assignedAtUtc, Guid? assignedBy = null, DateTimeOffset? expiresAtUtc = null)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAtUtc = assignedAtUtc;
        AssignedBy = assignedBy;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public DateTimeOffset AssignedAtUtc { get; private set; }

    public Guid? AssignedBy { get; private set; }

    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public User User { get; private set; } = null!;

    public Role Role { get; private set; } = null!;

    public bool IsExpired(DateTimeOffset now) => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= now;
}
