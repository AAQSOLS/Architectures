namespace ModularSaaS.Security.Abstractions;

public sealed record LoginFailureEvent(
    DateTimeOffset Timestamp,
    Guid? TenantId,
    string AttemptedEmail,
    string FailureReason,
    string? IpAddress,
    string? UserAgent) : SecurityEvent(Timestamp, "Auth.LoginFailure", TenantId, null, IpAddress, UserAgent);
