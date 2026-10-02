using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Errors;

public static class RoleErrors
{
    public static Error NameExists => Error.Conflict(
        "Role.NameExists",
        "A role with this name already exists in this tenant.");

    public static Error NotFound => Error.NotFound(
        "Role.NotFound",
        "The requested role was not found.");

    public static Error SystemProtected => Error.Conflict(
        "Role.SystemProtected",
        "System roles cannot be modified or deleted.");

    public static Error Validation(string message) => Error.Validation(
        "Role.Validation",
        message);
}
