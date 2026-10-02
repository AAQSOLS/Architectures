using System.Security.Claims;

namespace ModularSaaS.Security.Abstractions;

public sealed record TokenDescriptor(
    Guid? UserId,
    Guid? TenantId,
    string? Email = null,
    string? Name = null,
    IEnumerable<string>? Roles = null,
    IEnumerable<string>? Permissions = null,
    IEnumerable<Claim>? AdditionalClaims = null,
    TimeSpan? Lifetime = null,
    Guid? ImpersonatorUserId = null,
    string? ImpersonatorEmail = null);
