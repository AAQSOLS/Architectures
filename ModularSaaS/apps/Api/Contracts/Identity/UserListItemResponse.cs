using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Api.Contracts.Identity;

public sealed record UserListItemResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserStatus Status,
    IReadOnlyList<string> Roles,
    DateTimeOffset CreatedAtUtc);
