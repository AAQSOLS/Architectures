using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Application.Identity.Models;

public sealed record UserDetailsResult(
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

public sealed record UserListItem(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserStatus Status,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAtUtc);
