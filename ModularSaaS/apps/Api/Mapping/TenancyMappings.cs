using Mapster;
using ModularSaaS.Api.Contracts.Tenancy;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Models;

namespace ModularSaaS.Api.Mapping;

/// <summary>
/// Host-layer mappings between API contracts and Application Tenancy models.
/// </summary>
internal sealed class TenancyMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Request -> Input
        config.NewConfig<CreateTenantRequest, CreateTenantInput>();

        // Result -> Response (TenantListItem shares the same shape as TenantResult)
        config.NewConfig<TenantResult, TenantResponse>();
        config.NewConfig<TenantListItem, TenantResponse>();

        // Paged results
        config.NewConfig<PagedResult<TenantListItem>, PagedResult<TenantResponse>>();
    }
}
