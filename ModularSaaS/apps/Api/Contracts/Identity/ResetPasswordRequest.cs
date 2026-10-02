namespace ModularSaaS.Api.Contracts.Identity;

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);

