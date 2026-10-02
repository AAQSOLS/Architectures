using Mapster;
using ModularSaaS.Application.Tenancy.Models;
using ModularSaaS.Domain.Tenancy;

namespace ModularSaaS.Application.Tenancy.Mapping;

/// <summary>
/// Entity-to-result mappings for the Tenancy module.
/// </summary>
internal sealed class TenancyMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Tenant, TenantResult>();
    }
}
