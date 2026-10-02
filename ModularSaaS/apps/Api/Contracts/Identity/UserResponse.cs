using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Api.Contracts.Identity;

public sealed record UserResponse(
    Guid Id,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    UserStatus Status,
    DateTimeOffset CreatedAtUtc);
