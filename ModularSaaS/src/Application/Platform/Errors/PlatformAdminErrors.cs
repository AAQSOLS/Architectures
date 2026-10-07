using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Platform.Errors;

public static class PlatformAdminErrors
{
    public static readonly Error CannotDeactivateSelf =
        Error.Conflict("PlatformAdmin.CannotDeactivateSelf", "You cannot deactivate your own platform administrator account.");

    public static readonly Error CannotDeactivateLastSuperAdmin =
        Error.Conflict("PlatformAdmin.CannotDeactivateLastSuperAdmin", "Cannot deactivate the last remaining active platform administrator.");

    public static readonly Error NotFound =
        Error.NotFound("PlatformAdmin.NotFound", "Platform administrator was not found.");

    public static readonly Error EmailExists =
        Error.Conflict("PlatformAdmin.EmailExists", "A platform administrator with this email already exists.");

    public static readonly Error AlreadyActive =
        Error.Conflict("PlatformAdmin.AlreadyActive", "Platform administrator is already active.");

    public static readonly Error AlreadyInactive =
        Error.Conflict("PlatformAdmin.AlreadyInactive", "Platform administrator is already inactive.");

    public static Error Validation(string message) =>
        Error.Validation("PlatformAdmin.Validation", message);
}
