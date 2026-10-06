using ModularSaaS.Domain.Shared;
using ModularSaaS.Domain.Tenancy.Enums;
using ModularSaaS.Domain.Tenancy.Events;
using ModularSaaS.Domain.Tenancy.ValueObjects;

namespace ModularSaaS.Domain.Tenancy;

public class Tenant : AuditableEntity, IAggregateRoot
{
    private Tenant()
    {
    }

    public Tenant(string name, string identifier, TenantPlan plan = TenantPlan.Free)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var validIdentifier = TenantIdentifier.Create(identifier);

        Name = name.Trim();
        Identifier = validIdentifier.Value;
        Plan = plan;
        Status = TenantStatus.Active;

        RaiseDomainEvent(new TenantCreatedDomainEvent(Id, Identifier, Plan));
    }

    public Tenant(Guid id, string name, string identifier, TenantPlan plan = TenantPlan.Free)
        : this(name, identifier, plan)
    {
        Id = id;
    }

    public TenantIdentifier ToTenantIdentifier() => TenantIdentifier.Create(Identifier);

    public string Name { get; private set; } = string.Empty;

    public string Identifier { get; private set; } = string.Empty;

    public TenantStatus Status { get; private set; }

    public TenantPlan Plan { get; private set; }

    public void Activate()
    {
        if (Status != TenantStatus.Active)
        {
            Status = TenantStatus.Active;
            RaiseDomainEvent(new TenantStatusChangedDomainEvent(Id, Status));
        }
    }

    public void Suspend()
    {
        if (Status != TenantStatus.Suspended)
        {
            Status = TenantStatus.Suspended;
            RaiseDomainEvent(new TenantStatusChangedDomainEvent(Id, Status));
        }
    }

    public void ChangePlan(TenantPlan newPlan)
    {
        if (Plan != newPlan)
        {
            Plan = newPlan;
            RaiseDomainEvent(new TenantPlanChangedDomainEvent(Id, Plan));
        }
    }

    public void UpdateName(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        Name = newName.Trim();
    }
}
