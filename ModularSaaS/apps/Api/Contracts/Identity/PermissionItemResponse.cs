namespace ModularSaaS.Api.Contracts.Identity;

public sealed record PermissionItemResponse(
    Guid Id,
    string Code,
    string Name,
    string Description);

