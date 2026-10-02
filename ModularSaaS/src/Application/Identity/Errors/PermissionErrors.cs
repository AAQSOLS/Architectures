using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Errors;

public static class PermissionErrors
{
    public static Error NotFound => Error.NotFound(
        "Permission.NotFound",
        "One or more specified permissions were not found.");
}
