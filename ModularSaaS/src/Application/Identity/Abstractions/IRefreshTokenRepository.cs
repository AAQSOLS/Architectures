using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    public Task AddAsync(RefreshToken token, CancellationToken ct = default);

    public Task RevokeUserTokensAsync(Guid userId, string? replacedByToken = null, CancellationToken ct = default);
}
