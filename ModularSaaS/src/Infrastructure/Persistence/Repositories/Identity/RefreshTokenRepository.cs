using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Repositories.Identity;

internal sealed class RefreshTokenRepository(AppDbContext dbContext, IClock clock) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        await dbContext.RefreshTokens.AddAsync(token, ct);
    }

    public async Task RevokeUserTokensAsync(Guid userId, string? replacedByToken = null, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        var tokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > now)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.Revoke(now, replacedByToken);
        }
    }
}
