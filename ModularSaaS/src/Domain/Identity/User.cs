using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Identity.Events;
using ModularSaaS.Domain.Identity.ValueObjects;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Shared.ValueObjects;
using EmailVo = ModularSaaS.Domain.Shared.ValueObjects.Email;

namespace ModularSaaS.Domain.Identity;

public class User : AuditableEntity, ITenantEntity, IAggregateRoot
{
    private readonly List<UserRole> _roles = [];
    private readonly List<UserPermission> _permissions = [];

    private User()
    {
    }

    public User(Guid tenantId, string email, string passwordHash, string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var validEmail = EmailVo.Create(email);
        var fullName = FullName.Create(firstName, lastName);

        TenantId = tenantId;
        Email = validEmail.Value;
        PasswordHash = passwordHash;
        FirstName = fullName.FirstName;
        LastName = fullName.LastName;
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

    public UserId UserId => UserId.From(Id);

    public EmailVo ToEmail() => EmailVo.Create(Email);

    public FullName ToFullName() => FullName.Create(FirstName, LastName);

    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    public IReadOnlyCollection<UserPermission> Permissions => _permissions.AsReadOnly();

    public void AssignRole(Guid roleId, DateTimeOffset now, Guid? assignedBy = null, DateTimeOffset? expiresAtUtc = null)
    {
        if (Status != UserStatus.Active)
        {
            throw new InvalidOperationException($"Cannot assign roles to a user in '{Status}' status.");
        }

        var existing = _roles.Find(r => r.RoleId == roleId);
        if (existing is not null)
        {
            if (!existing.IsExpired(now))
            {
                return;
            }

            _roles.Remove(existing);
        }

        _roles.Add(new UserRole(Id, roleId, now, assignedBy, expiresAtUtc));
        RaiseDomainEvent(new UserRoleAssignedDomainEvent(Id, TenantId, roleId));
    }

    public void RemoveRole(Guid roleId)
    {
        var existing = _roles.Find(r => r.RoleId == roleId);
        if (existing is not null)
        {
            _roles.Remove(existing);
            RaiseDomainEvent(new UserRoleRemovedDomainEvent(Id, TenantId, roleId));
        }
    }

    public void SetDirectPermission(Guid permissionId, bool isGranted)
    {
        if (Status != UserStatus.Active)
        {
            throw new InvalidOperationException($"Cannot modify permissions for a user in '{Status}' status.");
        }

        var existing = _permissions.Find(p => p.PermissionId == permissionId);
        if (existing is not null)
        {
            existing.SetGranted(isGranted);
        }
        else
        {
            _permissions.Add(new UserPermission(Id, permissionId, isGranted));
        }

        RaiseDomainEvent(new UserPermissionChangedDomainEvent(Id, TenantId, permissionId, isGranted));
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        var fullName = FullName.Create(firstName, lastName);
        FirstName = fullName.FirstName;
        LastName = fullName.LastName;
    }

    public void UpdateProfile(FullName fullName)
    {
        ArgumentNullException.ThrowIfNull(fullName);
        FirstName = fullName.FirstName;
        LastName = fullName.LastName;
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
