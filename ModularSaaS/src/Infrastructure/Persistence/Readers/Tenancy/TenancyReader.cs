using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Application.Tenancy.Models;

namespace ModularSaaS.Infrastructure.Persistence.Readers.Tenancy;

internal sealed class TenancyReader(AppDbContext dbContext) : ITenancyReader
{
    public async Task<PagedResult<TenantListItem>> ListAsync(TenantFilter filter, CancellationToken ct = default)
    {
        var query = dbContext.Tenants.AsNoTracking().AsQueryable();

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
