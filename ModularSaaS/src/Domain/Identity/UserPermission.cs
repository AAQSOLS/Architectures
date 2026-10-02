namespace ModularSaaS.Domain.Identity;

public class UserPermission
{
    private UserPermission()
    {
    }

    public UserPermission(Guid userId, Guid permissionId, bool isGranted)
    {
        UserId = userId;
        PermissionId = permissionId;
        IsGranted = isGranted;
    }

    public Guid UserId { get; private set; }

    public Guid PermissionId { get; private set; }

    public bool IsGranted { get; private set; }

    public User User { get; private set; } = null!;

    public Permission Permission { get; private set; } = null!;

    public void SetGranted(bool granted)
    {
        IsGranted = granted;
    }
}
