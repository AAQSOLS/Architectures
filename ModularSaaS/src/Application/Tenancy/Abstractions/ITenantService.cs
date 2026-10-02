using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Models;

namespace ModularSaaS.Application.Tenancy.Abstractions;

public interface ITenantService
{
    public Task<Result<TenantResult>> CreateTenantAsync(CreateTenantInput input, CancellationToken ct = default);

    public Task<Result<TenantResult>> GetByIdAsync(Guid id, CancellationToken ct = default);

    public Task<Result<TenantResult>> GetByIdentifierAsync(string identifier, CancellationToken ct = default);

    public Task<Result<PagedResult<TenantListItem>>> ListTenantsAsync(TenantFilter filter, CancellationToken ct = default);

    public Task<Result> SuspendTenantAsync(Guid id, CancellationToken ct = default);

    public Task<Result> ActivateTenantAsync(Guid id, CancellationToken ct = default);
}
