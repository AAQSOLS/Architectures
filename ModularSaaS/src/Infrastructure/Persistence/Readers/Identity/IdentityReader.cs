using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Infrastructure.Persistence.Readers.Identity;

internal sealed class IdentityReader(AppDbContext dbContext, IClock clock) : IIdentityReader
{
    public async Task<PagedResult<UserListItem>> ListUsersAsync(Guid tenantId, PageRequest page, CancellationToken ct = default)
    {
        var query = dbContext.Users.AsNoTracking().Where(u => u.TenantId == tenantId);
        var total = await query.CountAsync(ct);

        var now = clock.UtcNow;
        var users = await query
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.Take)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Status,
                u.CreatedAtUtc,
                Roles = dbContext.UserRoles
                    .Where(ur => ur.UserId == u.Id && (!ur.ExpiresAtUtc.HasValue || ur.ExpiresAtUtc.Value > now))
                    .Select(ur => ur.Role.Name)
                    .ToList()
            })
            .ToListAsync(ct);

        var items = users.Select(u => new UserListItem(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.Status,
            u.Roles,
            u.CreatedAtUtc)).ToList();

        return new PagedResult<UserListItem>(items, total, page.PageNumber, page.PageSize);
    }
}
