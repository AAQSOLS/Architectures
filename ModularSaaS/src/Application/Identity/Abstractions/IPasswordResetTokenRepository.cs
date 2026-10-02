using ModularSaaS.Domain.Identity;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IPasswordResetTokenRepository
{
    public Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    public Task AddAsync(PasswordResetToken token, CancellationToken ct = default);

    public Task InvalidateUserTokensAsync(Guid userId, CancellationToken ct = default);
}
