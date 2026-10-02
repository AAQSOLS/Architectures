namespace ModularSaaS.Security.Abstractions;

public sealed record SecurityContextSnapshot(
    Guid? TenantId,
    Guid? UserId,
    string? Email = null,
    IReadOnlyList<string>? Roles = null,
    bool IsPlatformAdmin = false);
