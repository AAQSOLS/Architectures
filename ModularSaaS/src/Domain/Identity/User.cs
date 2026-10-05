using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Identity.Events;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity;

public class User : AuditableEntity, ITenantEntity, IAggregateRoot
{
    private User()
    {
    }

    public User(Guid tenantId, string email, string passwordHash, string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        TenantId = tenantId;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Status = UserStatus.Active;
        EmailConfirmed = false;
        AccessFailedCount = 0;

        RaiseDomainEvent(new UserCreatedDomainEvent(Id, TenantId, Email, FirstName, LastName));
    }

    public Guid TenantId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    public bool EmailConfirmed { get; private set; }

    public int AccessFailedCount { get; private set; }

    public DateTimeOffset? LockoutEndUtc { get; private set; }

    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    public void UpdateProfile(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void SetPasswordHash(string newHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newHash);
        PasswordHash = newHash;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void RecordFailedLogin(int maxFailedAttempts, TimeSpan lockoutDuration, DateTimeOffset now)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxFailedAttempts)
        {
            LockoutEndUtc = now.Add(lockoutDuration);
            Status = UserStatus.Locked;
        }
    }

    public void RecordSuccessfulLogin(DateTimeOffset now)
    {
        AccessFailedCount = 0;
        LockoutEndUtc = null;
        LastLoginAtUtc = now;
        if (Status == UserStatus.Locked)
        {
            Status = UserStatus.Active;
        }
    }
}
