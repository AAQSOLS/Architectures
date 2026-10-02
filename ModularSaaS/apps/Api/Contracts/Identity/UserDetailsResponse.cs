using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Api.Contracts.Identity;

public sealed record UserDetailsResponse(
    Guid Id,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    UserStatus Status,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    DateTimeOffset? LastLoginAtUtc,
    DateTimeOffset CreatedAtUtc);

