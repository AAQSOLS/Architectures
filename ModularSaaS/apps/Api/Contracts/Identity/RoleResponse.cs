namespace ModularSaaS.Api.Contracts.Identity;

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsSystem,
    bool IsDefault);

