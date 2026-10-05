using Microsoft.AspNetCore.Authorization;

namespace ModularSaaS.Security.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission ?? throw new ArgumentNullException(nameof(permission));
}
