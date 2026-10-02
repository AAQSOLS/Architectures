namespace ModularSaaS.Api.Contracts.Platform;

public sealed record ImpersonationResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);
