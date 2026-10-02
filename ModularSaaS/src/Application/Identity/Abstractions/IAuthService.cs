using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Abstractions;

public interface IAuthService
{
    public Task<Result<AuthTokensResult>> LoginAsync(LoginInput input, string? ipAddress = null, CancellationToken ct = default);

    public Task<Result<AuthTokensResult>> RefreshTokenAsync(RefreshTokenInput input, string? ipAddress = null, CancellationToken ct = default);

    public Task<Result> RevokeTokenAsync(string token, CancellationToken ct = default);

    public Task<Result> ForgotPasswordAsync(ForgotPasswordInput input, CancellationToken ct = default);

    public Task<Result> ResetPasswordAsync(ResetPasswordInput input, CancellationToken ct = default);
}
