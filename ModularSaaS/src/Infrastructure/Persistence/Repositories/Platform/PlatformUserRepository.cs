using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Domain.Identity.Enums;
using ModularSaaS.Domain.Platform;

namespace ModularSaaS.Infrastructure.Persistence.Repositories.Platform;

internal sealed class PlatformUserRepository(AppDbContext dbContext)
    : EfRepository<PlatformUser>(dbContext), IPlatformUserRepository
{
    public async Task<PlatformUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await DbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await DbContext.PlatformUsers
            .AnyAsync(u => u.Email == normalizedEmail, ct);
    }

    public async Task<IReadOnlyList<PlatformUser>> ListAsync(CancellationToken ct = default)
    {
        return await DbContext.PlatformUsers
            .OrderBy(u => u.Email)
            .ToListAsync(ct);
    }

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
    {
        return await DbContext.PlatformUsers
            .CountAsync(u => u.Status == UserStatus.Active, ct);
    }
}
