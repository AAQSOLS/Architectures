using ModularSaaS.Domain.Identity.Enums;

namespace ModularSaaS.Application.Platform.Models;

public sealed record CreatePlatformAdminInput(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record PlatformAdminResult(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserStatus Status,
    DateTimeOffset CreatedAtUtc);
