using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Application.Identity.Models;

public sealed record RegisterUserInput(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid? RoleId = null,
    Guid? TenantId = null);

public sealed record UserResult(
    Guid Id,
    Guid TenantId,
    string Email,
    string FirstName,
    string LastName,
    UserStatus Status,
    DateTimeOffset CreatedAtUtc);
