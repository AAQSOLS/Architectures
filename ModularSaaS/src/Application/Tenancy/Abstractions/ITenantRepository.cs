using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Models;
using ModularSaaS.Domain.Tenancy;

namespace ModularSaaS.Application.Tenancy.Abstractions;

public interface ITenantRepository : IRepository<Tenant>
{
    public Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken ct = default);

    public Task<bool> IdentifierExistsAsync(string identifier, CancellationToken ct = default);

    public Task<PagedResult<TenantListItem>> ListAsync(TenantFilter filter, CancellationToken ct = default);
}
