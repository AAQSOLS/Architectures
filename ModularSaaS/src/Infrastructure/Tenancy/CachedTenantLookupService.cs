using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Application.Tenancy.Models;
using ModularSaaS.Domain.Tenancy.Enums;
using ModularSaaS.Infrastructure.Persistence;

namespace ModularSaaS.Infrastructure.Tenancy;

internal sealed class CachedTenantLookupService(
    AppDbContext dbContext,
    IMemoryCache cache) : ITenantLookupService
{
    private const int SlidingExpirationMinutes = 15;
    private static readonly CompositeFormat CacheKeyFormat = CompositeFormat.Parse("tenant:lookup:{0}");

    private static string GetCacheKey(Guid tenantId) =>
        string.Format(CultureInfo.InvariantCulture, CacheKeyFormat, tenantId);

    public async Task<bool> IsTenantActiveAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await FindByIdAsync(tenantId, ct);
        return tenant is not null && tenant.Status == TenantStatus.Active && !tenant.IsDeleted;
    }

    public async Task<TenantLookupResult?> FindByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        var key = GetCacheKey(tenantId);
        if (cache.TryGetValue(key, out TenantLookupResult? cached) && cached is not null)
        {
            return cached;
        }

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantId)
            .Select(t => new TenantLookupResult(
                t.Id,
                t.Identifier,
                t.Name,
                t.Status,
                t.IsDeleted))
            .FirstOrDefaultAsync(ct);

        if (tenant is not null)
        {
            cache.Set(key, tenant, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(SlidingExpirationMinutes)
            });
        }

        return tenant;
    }

    public Task InvalidateAsync(Guid tenantId, CancellationToken ct = default)
    {
        cache.Remove(GetCacheKey(tenantId));
        return Task.CompletedTask;
    }
}
