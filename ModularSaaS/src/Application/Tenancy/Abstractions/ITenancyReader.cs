using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Models;

namespace ModularSaaS.Application.Tenancy.Abstractions;

public interface ITenancyReader
{
    public Task<PagedResult<TenantListItem>> ListAsync(TenantFilter filter, CancellationToken ct = default);
}
