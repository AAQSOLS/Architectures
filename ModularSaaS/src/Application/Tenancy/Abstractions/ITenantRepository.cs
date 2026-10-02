using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Tenancy;

namespace ModularSaaS.Application.Tenancy.Abstractions;

public interface ITenantRepository : IRepository<Tenant>
{
    public Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken ct = default);

    public Task<bool> IdentifierExistsAsync(string identifier, CancellationToken ct = default);
}
