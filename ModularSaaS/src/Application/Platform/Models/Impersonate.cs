namespace ModularSaaS.Application.Platform.Models;

public sealed record ImpersonateInput(
    Guid TargetUserId);

public sealed record ImpersonationResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);
