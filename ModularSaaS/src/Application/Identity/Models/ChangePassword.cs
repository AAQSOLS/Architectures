namespace ModularSaaS.Application.Identity.Models;

public sealed record ChangePasswordInput(
    string CurrentPassword,
    string NewPassword);
