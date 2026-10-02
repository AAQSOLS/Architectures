namespace ModularSaaS.Security.Abstractions;

public sealed record SecurityTokenResult(
    string Token,
    string TokenId,
    DateTimeOffset ExpiresAtUtc);
