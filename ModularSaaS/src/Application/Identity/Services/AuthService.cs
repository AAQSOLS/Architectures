using FluentValidation;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Errors;
using ModularSaaS.Application.Identity.Models;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Application.Shared.Models;
using ModularSaaS.Domain.Identity;
using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Application.Identity.Services;

internal sealed class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IPasswordHasher passwordHasher,
    ITenantTokenService tenantTokenService,
    ISecureTokenGenerator secureTokenGenerator,
    IClock clock,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IEmailSender emailSender,
    IValidator<LoginInput> loginValidator,
    IValidator<ForgotPasswordInput> forgotPasswordValidator,
    IValidator<ResetPasswordInput> resetPasswordValidator) : IAuthService
{
    public async Task<Result<AuthTokensResult>> LoginAsync(LoginInput input, string? ipAddress = null, CancellationToken ct = default)
    {
        var validation = await loginValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var tenantId = tenantContext.RequireTenantId();
        var user = await userRepository.GetByEmailAsync(tenantId, input.Email, ct);
        if (user is null)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidCredentials);
        }

        var now = clock.UtcNow;
        if (user.Status == UserStatus.Locked && user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > now)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.AccountLocked);
        }

        if (!passwordHasher.Verify(input.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(
                SecurityPolicies.MaxFailedAccessAttempts,
                TimeSpan.FromMinutes(SecurityPolicies.LockoutDurationMinutes),
                now);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<AuthTokensResult>(AuthErrors.InvalidCredentials);
        }

        user.RecordSuccessfulLogin(now);

        var roles = await userRepository.GetUserRoleNamesAsync(user.Id, ct);
        var permissions = await userRepository.GetEffectivePermissionsAsync(tenantId, user.Id, ct);

        var accessToken = tenantTokenService.GenerateAccessToken(user.Id, user.Email, tenantId, roles, permissions);
        var rawRefreshToken = secureTokenGenerator.GenerateRefreshToken();
        var refreshExpires = now.AddDays(SecurityPolicies.RefreshTokenLifetimeDays);

        var refreshToken = new RefreshToken(tenantId, user.Id, rawRefreshToken, refreshExpires, now, ipAddress);
        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthTokensResult(accessToken, rawRefreshToken, refreshExpires);
    }

    public async Task<Result<AuthTokensResult>> RefreshTokenAsync(RefreshTokenInput input, string? ipAddress = null, CancellationToken ct = default)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(input.RefreshToken, ct);
        if (existingToken is null)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.TokenNotFound);
        }

        var now = clock.UtcNow;
        if (!existingToken.IsActive(now))
        {
            if (existingToken.RevokedAtUtc.HasValue)
            {
                // Security: revoke all tokens for this user (possible replay attack)
                await refreshTokenRepository.RevokeUserTokensAsync(existingToken.UserId, SecurityPolicies.ReplayAttackRevokeReason, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }

            return Result.Failure<AuthTokensResult>(AuthErrors.TokenExpired);
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, ct);
        if (user is null)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.UserNotFound);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthTokensResult>(AuthErrors.UserInactive);
        }

        var tenantId = existingToken.TenantId;
        var roles = await userRepository.GetUserRoleNamesAsync(user.Id, ct);
        var permissions = await userRepository.GetEffectivePermissionsAsync(tenantId, user.Id, ct);

        var newAccessToken = tenantTokenService.GenerateAccessToken(user.Id, user.Email, tenantId, roles, permissions);
        var newRefreshTokenRaw = secureTokenGenerator.GenerateRefreshToken();
        var refreshExpires = now.AddDays(SecurityPolicies.RefreshTokenLifetimeDays);

        existingToken.Revoke(now, newRefreshTokenRaw);

        var newRefreshToken = new RefreshToken(tenantId, user.Id, newRefreshTokenRaw, refreshExpires, now, ipAddress);
        await refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new AuthTokensResult(newAccessToken, newRefreshTokenRaw, refreshExpires);
    }

    public async Task<Result> RevokeTokenAsync(string token, CancellationToken ct = default)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(token, ct);
        if (existingToken is null || !existingToken.IsActive(clock.UtcNow))
        {
            return Result.Failure(AuthErrors.TokenNotFound);
        }

        existingToken.Revoke(clock.UtcNow, SecurityPolicies.UserLogoutRevokeReason);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordInput input, CancellationToken ct = default)
    {
        var validation = await forgotPasswordValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure(AuthErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var tenantId = tenantContext.RequireTenantId();
        var user = await userRepository.GetByEmailAsync(tenantId, input.Email, ct);
        if (user is null)
        {
            // Return success to prevent email enumeration
            return Result.Success();
        }

        await passwordResetTokenRepository.InvalidateUserTokensAsync(user.Id, ct);

        var rawToken = secureTokenGenerator.GeneratePasswordResetToken();
        var tokenHash = secureTokenGenerator.HashToken(rawToken);
        var now = clock.UtcNow;
        var resetToken = new PasswordResetToken(tenantId, user.Id, tokenHash, now.AddMinutes(SecurityPolicies.PasswordResetTokenLifetimeMinutes), now);

        await passwordResetTokenRepository.AddAsync(resetToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            user.Email,
            "Reset your password",
            $"Use token: {rawToken} to reset your password. Valid for {SecurityPolicies.PasswordResetTokenLifetimeMinutes} minutes.",
            ct);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordInput input, CancellationToken ct = default)
    {
        var validation = await resetPasswordValidator.ValidateAsync(input, ct);
        if (!validation.IsValid)
        {
            return Result.Failure(AuthErrors.Validation(validation.Errors[0].ErrorMessage));
        }

        var tenantId = tenantContext.RequireTenantId();
        var user = await userRepository.GetByEmailAsync(tenantId, input.Email, ct);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var now = clock.UtcNow;
        var tokenHash = secureTokenGenerator.HashToken(input.Token);
        var resetToken = await passwordResetTokenRepository.GetByHashAsync(tokenHash, ct);
        if (resetToken is null || !resetToken.IsValid(now) || resetToken.UserId != user.Id)
        {
            return Result.Failure(AuthErrors.InvalidResetToken);
        }

        var newPasswordHash = passwordHasher.Hash(input.NewPassword);
        user.SetPasswordHash(newPasswordHash);
        resetToken.Use(now);

        // Invalidate active refresh tokens
        await refreshTokenRepository.RevokeUserTokensAsync(user.Id, SecurityPolicies.PasswordResetRevokeReason, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
