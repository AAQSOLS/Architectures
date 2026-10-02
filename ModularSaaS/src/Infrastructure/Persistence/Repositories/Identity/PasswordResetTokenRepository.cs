using Microsoft.EntityFrameworkCore;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Infrastructure.Persistence.Repositories.Identity;

internal sealed class PasswordResetTokenRepository(AppDbContext dbContext, IClock clock) : IPasswordResetTokenRepository
{
    public async Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(prt => prt.TokenHash == tokenHash, ct);
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        await dbContext.PasswordResetTokens.AddAsync(token, ct);
    }

    public async Task InvalidateUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var now = clock.UtcNow;
        var tokens = await dbContext.PasswordResetTokens
            .Where(prt => prt.UserId == userId && prt.UsedAtUtc == null && prt.ExpiresAtUtc > now)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.Use(now);
        }
    }
}
