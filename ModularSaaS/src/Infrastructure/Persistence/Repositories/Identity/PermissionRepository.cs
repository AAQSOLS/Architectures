using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Repositories.Identity;

internal sealed class PermissionRepository(AppDbContext dbContext) : IPermissionRepository
{
    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Code)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Permission>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        var codeSet = codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return await dbContext.Permissions
            .Where(p => codeSet.Contains(p.Code))
            .ToListAsync(ct);
    }

    public async Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await dbContext.Permissions
            .FirstOrDefaultAsync(p => p.Code == code, ct);
    }
}
