namespace ModularSaaS.Security.Abstractions;

public abstract record SecurityEvent(
    DateTimeOffset Timestamp,
    string EventType,
    Guid? TenantId,
    Guid? UserId,
    string? IpAddress,
    string? UserAgent);
