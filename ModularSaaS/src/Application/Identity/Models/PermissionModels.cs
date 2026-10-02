namespace ModularSaaS.Application.Identity.Models;

public sealed record PermissionItem(
    Guid Id,
    string Code,
    string Name,
    string Description);

public sealed record PermissionGroupResult(
    string Module,
    IReadOnlyList<PermissionItem> Permissions);
