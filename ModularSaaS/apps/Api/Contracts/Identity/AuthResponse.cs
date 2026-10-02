namespace ModularSaaS.Api.Contracts.Identity;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);

