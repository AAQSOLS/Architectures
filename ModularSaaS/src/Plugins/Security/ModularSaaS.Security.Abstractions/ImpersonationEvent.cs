namespace ModularSaaS.Security.Abstractions;

public sealed record ImpersonationEvent(
    DateTimeOffset Timestamp,
    Guid? TenantId,
    Guid? UserId,
    Guid ImpersonatorUserId,
    string Action,
    string? IpAddress,
    string? UserAgent) : SecurityEvent(Timestamp, "Auth.Impersonation", TenantId, UserId, IpAddress, UserAgent);
