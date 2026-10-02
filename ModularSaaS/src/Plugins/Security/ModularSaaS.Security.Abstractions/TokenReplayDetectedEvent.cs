namespace ModularSaaS.Security.Abstractions;

public sealed record TokenReplayDetectedEvent(
    DateTimeOffset Timestamp,
    Guid? TenantId,
    Guid? UserId,
    string TokenId,
    string? IpAddress,
    string? UserAgent) : SecurityEvent(Timestamp, "Auth.TokenReplayDetected", TenantId, UserId, IpAddress, UserAgent);
