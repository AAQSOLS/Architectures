namespace ModularSaaS.Api.Contracts.Identity;

public sealed record PermissionGroupResponse(
    string Module,
    IReadOnlyList<PermissionItemResponse> Permissions);

