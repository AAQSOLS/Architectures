using System.Globalization;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Shared.Abstractions;

namespace ModularSaaS.Infrastructure.Caching;

internal sealed class PermissionCache(IMemoryCache cache, IServiceProvider serviceProvider) : IPermissionCache
{
    private const int SlidingExpirationMinutes = 15;
    private const double CompactPercentage = 0.25;
    private static readonly CompositeFormat CacheKeyFormat = CompositeFormat.Parse("tenant:{0}:user:{1}:perms");

    private static string GetUserCacheKey(Guid tenantId, Guid userId) =>
        string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, tenantId, userId);

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var key = GetUserCacheKey(tenantId, userId);
        if (cache.TryGetValue(key, out IReadOnlySet<string>? cached) && cached is not null)
        {
            return cached;
        }

        using var scope = serviceProvider.CreateScope();
        var userPermissionReader = scope.ServiceProvider.GetRequiredService<IUserPermissionReader>();

        var permissions = await userPermissionReader.GetEffectivePermissionsAsync(tenantId, userId, ct);

        cache.Set(key, permissions, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(SlidingExpirationMinutes)
        });

        return permissions;
    }

    public Task InvalidateAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        cache.Remove(GetUserCacheKey(tenantId, userId));
        return Task.CompletedTask;
    }

    public Task InvalidateRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        if (cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(CompactPercentage);
        }

        return Task.CompletedTask;
    }
}
