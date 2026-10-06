using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Shared.ValueObjects;
using EmailVo = ModularSaaS.Domain.Shared.ValueObjects.Email;

namespace ModularSaaS.Domain.Platform;

public class PlatformUser : AuditableEntity, IAggregateRoot
{
    private PlatformUser()
    {
    }

    public PlatformUser(string email, string passwordHash, string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var validEmail = EmailVo.Create(email);
        var fullName = FullName.Create(firstName, lastName);

        Email = validEmail.Value;
        PasswordHash = passwordHash;
        FirstName = fullName.FirstName;
        LastName = fullName.LastName;
        Status = UserStatus.Active;
    }

    public EmailVo ToEmail() => EmailVo.Create(Email);

    public FullName ToFullName() => FullName.Create(FirstName, LastName);

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; }

    public void SetPasswordHash(string newHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newHash);
        PasswordHash = newHash;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }
}
