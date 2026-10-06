using ModularSaaS.Domain.Identity.Events;
using ModularSaaS.Domain.Identity.ValueObjects;
using ModularSaaS.Domain.Shared;

namespace ModularSaaS.Domain.Identity;

public class Role : AuditableEntity, ITenantEntity, IAggregateRoot
{
    private readonly List<RolePermission> _permissions = [];

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

    public RoleId RoleId => RoleId.From(Id);

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsSystem { get; private set; }

    public bool IsDefault { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    public void AssignPermission(Guid permissionId)
    {
        if (IsSystem)
        {
            throw new InvalidOperationException(SystemRoleModificationError);
        }

        if (_permissions.TrueForAll(p => p.PermissionId != permissionId))
        {
            _permissions.Add(new RolePermission(Id, permissionId));
            RaiseDomainEvent(new RolePermissionsUpdatedDomainEvent(Id, TenantId));
        }
    }

    public void RemovePermission(Guid permissionId)
    {
        if (IsSystem)
        {
            throw new InvalidOperationException(SystemRoleModificationError);
        }

        var existing = _permissions.Find(p => p.PermissionId == permissionId);
        if (existing is not null)
        {
            _permissions.Remove(existing);
            RaiseDomainEvent(new RolePermissionsUpdatedDomainEvent(Id, TenantId));
        }
    }

    public void SetPermissions(IEnumerable<Guid> permissionIds)
    {
        if (IsSystem)
        {
            throw new InvalidOperationException(SystemRoleModificationError);
        }

        ArgumentNullException.ThrowIfNull(permissionIds);

        _permissions.Clear();
        foreach (var pid in permissionIds.Distinct())
        {
            _permissions.Add(new RolePermission(Id, pid));
        }

        RaiseDomainEvent(new RolePermissionsUpdatedDomainEvent(Id, TenantId));
    }

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
