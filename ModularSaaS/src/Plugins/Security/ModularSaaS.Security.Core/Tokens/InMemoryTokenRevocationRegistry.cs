using System.Collections.Concurrent;
using ModularSaaS.Security.Abstractions;

namespace ModularSaaS.Security.Core.Tokens;

public sealed class InMemoryTokenRevocationRegistry(TimeProvider? timeProvider = null) : ITokenRevocationRegistry
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _revokedTokens = new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public ValueTask<bool> IsRevokedAsync(string tokenId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
        {
            return ValueTask.FromResult(true);
        }

        if (_revokedTokens.TryGetValue(tokenId, out var expiresAt))
        {
            if (expiresAt > _timeProvider.GetUtcNow())
            {
                return ValueTask.FromResult(true);
            }

            _revokedTokens.TryRemove(tokenId, out _);
        }

        return ValueTask.FromResult(false);
    }

    public ValueTask RevokeAsync(string tokenId, DateTimeOffset expiresAtUtc, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(tokenId))
        {
            _revokedTokens[tokenId] = expiresAtUtc;
        }

        return ValueTask.CompletedTask;
    }
}
