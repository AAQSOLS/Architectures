namespace ModularSaaS.Security.Core.Tokens;

public sealed record JwtServiceOptions
{
    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKey { get; init; } = string.Empty;

    public TimeSpan DefaultLifetime { get; init; } = TimeSpan.FromMinutes(15);

    public TimeSpan ClockSkew { get; init; } = TimeSpan.Zero;

    public IReadOnlyList<string> PreviousValidationKeys { get; init; } = [];
}
