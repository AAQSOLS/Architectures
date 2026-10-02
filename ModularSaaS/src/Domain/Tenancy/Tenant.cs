using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Domain.Tenancy;

public class Tenant : AuditableEntity, IAggregateRoot
{
    private Tenant()
    {
    }

    public Tenant(string name, string identifier, TenantPlan plan = TenantPlan.Free)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        Name = name.Trim();
        Identifier = identifier.Trim().ToLowerInvariant();
        Plan = plan;
        Status = TenantStatus.Active;
    }

    public Tenant(Guid id, string name, string identifier, TenantPlan plan = TenantPlan.Free)
        : this(name, identifier, plan)
    {
        Id = id;
    }

    public string Name { get; private set; } = string.Empty;

    public string Identifier { get; private set; } = string.Empty;

    public TenantStatus Status { get; private set; }

    public TenantPlan Plan { get; private set; }

    public void Activate()
    {
        Status = TenantStatus.Active;
    }

    public void Suspend()
    {
        Status = TenantStatus.Suspended;
    }

    public void ChangePlan(TenantPlan newPlan)
    {
        Plan = newPlan;
    }

    public void UpdateName(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        Name = newName.Trim();
    }
}
