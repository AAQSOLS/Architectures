namespace ModularSaaS.Api.Contracts.Identity;

public sealed record CreateRoleRequest(
    string Name,
    string Description,
    IReadOnlyList<string> Permissions);

