using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Application.Tenancy.Models;
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

    public async Task<PagedResult<TenantListItem>> ListAsync(TenantFilter filter, CancellationToken ct = default)
    {
        var query = DbContext.Tenants.AsNoTracking().AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(t => t.Name.Contains(term) || t.Identifier.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var page = filter.Page ?? new PageRequest();

        var items = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.Take)
            .Select(t => new TenantListItem(t.Id, t.Name, t.Identifier, t.Status, t.Plan, t.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResult<TenantListItem>(items, total, page.PageNumber, page.PageSize);
    }
}
