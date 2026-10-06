using Bogus;
using ModularSaaS.Domain.Tenancy;
using ModularSaaS.Domain.Tenancy.Enums;

namespace ModularSaaS.Testing.Shared.Builders;

public sealed class TenantBuilder
{
    private readonly Faker _faker = new();
    private Guid? _id;
    private string? _name;
    private string? _identifier;
    private TenantPlan _plan = TenantPlan.Free;

    public TenantBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public TenantBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TenantBuilder WithIdentifier(string identifier)
    {
        _identifier = identifier;
        return this;
    }

    public TenantBuilder WithPlan(TenantPlan plan)
    {
        _plan = plan;
        return this;
    }

    public Tenant Build()
    {
        var name = _name ?? _faker.Company.CompanyName();
        var identifier = _identifier ?? _faker.Internet.DomainWord().ToLowerInvariant();

        return _id.HasValue
            ? new Tenant(_id.Value, name, identifier, _plan)
            : new Tenant(name, identifier, _plan);
    }
}
