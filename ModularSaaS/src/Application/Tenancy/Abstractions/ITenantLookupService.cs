using ModularSaaS.Application.Tenancy.Models;

namespace ModularSaaS.Application.Tenancy.Abstractions;

public interface ITenantLookupService
{
    public Task<bool> IsTenantActiveAsync(Guid tenantId, CancellationToken ct = default);

    public Task<TenantLookupResult?> FindByIdAsync(Guid tenantId, CancellationToken ct = default);

    public Task InvalidateAsync(Guid tenantId, CancellationToken ct = default);
}
