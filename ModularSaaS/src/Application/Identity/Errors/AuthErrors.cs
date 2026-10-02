using ModularSaaS.Application.Shared.Models;

namespace ModularSaaS.Application.Identity.Errors;

public static class AuthErrors
{
    public static Error InvalidCredentials => Error.Unauthorized(
        "Auth.InvalidCredentials",
        "Invalid email or password.");

    public static Error AccountInactive => Error.Unauthorized(
        "Auth.AccountInactive",
        "The user account is suspended or pending activation.");

    public static Error AccountLocked => Error.Unauthorized(
        "Auth.AccountLocked",
        "Account is locked due to too many failed attempts. Try again later.");

    public static Error InvalidRefreshToken => Error.Unauthorized(
        "Auth.InvalidToken",
        "The provided refresh token is invalid or missing.");

    public static Error TokenExpired => Error.Unauthorized(
        "Auth.TokenExpired",
        "The refresh token is expired or revoked.");

    public static Error UserInactive => Error.Unauthorized(
        "Auth.UserInactive",
        "User account is disabled or missing.");

    public static Error TokenNotFound => Error.NotFound(
        "Auth.TokenNotFound",
        "Token not found or already inactive.");

    public static Error InvalidResetToken => Error.Unauthorized(
        "Auth.InvalidToken",
        "Reset token is invalid or expired.");

    public static Error UserNotFound => Error.NotFound(
        "Auth.UserNotFound",
        "User was not found.");

    public static Error Validation(string message) => Error.Validation(
        "Auth.Validation",
        message);
}
