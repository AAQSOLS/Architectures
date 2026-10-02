namespace ModularSaaS.Application.Platform.Models;

public sealed record PlatformLoginInput(
    string Email,
    string Password);

public sealed record PlatformAuthResult(
    string AccessToken,
    Guid UserId,
    string Email,
    string FullName);
