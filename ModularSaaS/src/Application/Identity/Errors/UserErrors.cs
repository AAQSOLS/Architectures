using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Errors;

public static class UserErrors
{
    public static Error EmailExists => Error.Conflict(
        "User.EmailExists",
        "A user with this email address already exists.");

    public static Error NotFound => Error.NotFound(
        "User.NotFound",
        "The requested user was not found.");

    public static Error Validation(string message) => Error.Validation(
        "User.Validation",
        message);
}
