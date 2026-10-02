using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Platform.Errors;

public static class PlatformAuthErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("PlatformAuth.InvalidCredentials", "Invalid platform admin credentials.");

    public static readonly Error Forbidden =
        Error.Unauthorized("PlatformAuth.Forbidden", "Only platform administrators can perform impersonation.");

    public static readonly Error TargetUserNotFound =
        Error.NotFound("PlatformAuth.TargetUserNotFound", "Target user does not exist in the specified tenant.");

    public static Error Validation(string message) =>
        Error.Validation("PlatformAuth.Validation", message);
}
