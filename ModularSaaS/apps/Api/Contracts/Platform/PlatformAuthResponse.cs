namespace ModularSaaS.Api.Contracts.Platform;

public sealed record PlatformAuthResponse(
    string AccessToken,
    Guid UserId,
    string Email,
    string FullName);
