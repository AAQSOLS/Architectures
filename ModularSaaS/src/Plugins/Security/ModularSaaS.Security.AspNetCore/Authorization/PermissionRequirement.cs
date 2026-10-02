using Microsoft.AspNetCore.Authorization;

namespace ModularSaaS.Security.AspNetCore.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
