namespace ModularSaaS.Api.Contracts.Identity;

public sealed record AssignRoleRequest(
    Guid RoleId,
    DateTimeOffset? ExpiresAtUtc = null);

