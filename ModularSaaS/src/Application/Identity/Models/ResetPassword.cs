namespace ModularSaaS.Application.Identity.Models;

public sealed record ResetPasswordInput(
    string Email,
    string Token,
    string NewPassword);
