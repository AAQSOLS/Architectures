namespace ModularSaaS.Application.Identity.Models;

public sealed record CreateRoleInput(
    string Name,
    string Description,
    IReadOnlyList<string> Permissions);

public sealed record UpdateRoleInput(
    string Name,
    string Description,
    IReadOnlyList<string> Permissions);

public sealed record RoleResult(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsDefault);

public sealed record RoleDetailsResult(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsDefault,
    IReadOnlyList<string> Permissions);

public sealed record AssignRoleInput(
    Guid RoleId,
    DateTimeOffset? ExpiresAtUtc = null);
