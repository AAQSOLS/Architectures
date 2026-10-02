namespace ModularSaaS.Api.Contracts.Identity;

public sealed record UpdateRoleRequest(
    string Name,
    string Description,
    IReadOnlyList<string> Permissions);

