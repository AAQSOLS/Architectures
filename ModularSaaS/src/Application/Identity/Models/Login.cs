namespace ModularSaaS.Application.Identity.Models;

public sealed record LoginInput(
    string Email,
    string Password);

public sealed record AuthTokensResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
