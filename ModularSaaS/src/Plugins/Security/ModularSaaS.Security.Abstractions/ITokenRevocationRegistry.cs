namespace ModularSaaS.Security.Abstractions;

public interface ITokenRevocationRegistry
{
    public ValueTask<bool> IsRevokedAsync(string tokenId, CancellationToken ct = default);

    public ValueTask RevokeAsync(string tokenId, DateTimeOffset expiresAtUtc, CancellationToken ct = default);
}
