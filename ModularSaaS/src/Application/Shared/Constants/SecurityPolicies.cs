namespace ModularSaaS.Application.Shared.Constants;

public static class SecurityPolicies
{
    public const int MaxFailedAccessAttempts = 5;
    public const int LockoutDurationMinutes = 15;
    public const int RefreshTokenLifetimeDays = 7;
    public const int PasswordResetTokenLifetimeMinutes = 30;

    public const int PlatformTokenLifetimeHours = 8;
    public const int ImpersonationTokenLifetimeMinutes = 15;
    public const int RefreshTokenByteCount = 64;
    public const int PasswordResetTokenByteCount = 32;

    public const string ReplayAttackRevokeReason = "Replay attack detected";
    public const string UserLogoutRevokeReason = "User logout";
    public const string PasswordResetRevokeReason = "Password reset";
}
