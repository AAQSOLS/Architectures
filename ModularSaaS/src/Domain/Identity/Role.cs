using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity;

public class Role : AuditableEntity, ITenantEntity, IAggregateRoot
{
    private Role()
    {
    }

    public Role(Guid tenantId, string name, string description = "", bool isSystem = false, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        TenantId = tenantId;
        Name = name.Trim();
        Description = description.Trim();
        IsSystem = isSystem;
        IsDefault = isDefault;
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsSystem { get; private set; }

    public bool IsDefault { get; private set; }

    private const string SystemRoleModificationError = "System roles cannot be renamed or modified.";

    public void UpdateDetails(string name, string description)
    {
        if (IsSystem)
        {
            throw new InvalidOperationException(SystemRoleModificationError);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Description = description.Trim();
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}
