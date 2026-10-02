using Microsoft.AspNetCore.Authorization;

namespace ModularSaaS.Security.AspNetCore.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission)
{
    public string Permission => Policy!;
}
