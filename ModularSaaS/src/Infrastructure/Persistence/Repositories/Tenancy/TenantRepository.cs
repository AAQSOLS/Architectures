using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Domain.Tenancy;

namespace ModularSaaS.Infrastructure.Persistence.Repositories.Tenancy;

internal sealed class TenantRepository(AppDbContext dbContext)
    : EfRepository<Tenant>(dbContext), ITenantRepository
{
    public async Task<Tenant?> GetByIdentifierAsync(string identifier, CancellationToken ct = default)
    {
        return await DbContext.Tenants
            .FirstOrDefaultAsync(t => t.Identifier == identifier, ct);
    }

    public async Task<bool> IdentifierExistsAsync(string identifier, CancellationToken ct = default)
    {
        return await DbContext.Tenants
            .AnyAsync(t => t.Identifier == identifier, ct);
    }
}
