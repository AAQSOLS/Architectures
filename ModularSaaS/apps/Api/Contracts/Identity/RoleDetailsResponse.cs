namespace ModularSaaS.Api.Contracts.Identity;

public sealed record RoleDetailsResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsDefault,
    IReadOnlyList<string> Permissions);

