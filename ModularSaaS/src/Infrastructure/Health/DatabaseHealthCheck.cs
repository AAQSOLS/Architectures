using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModularSaaS.Infrastructure.Persistence;

namespace ModularSaaS.Infrastructure.Health;

internal sealed class DatabaseHealthCheck(AppDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection verified.")
                : HealthCheckResult.Unhealthy("Database could not be reached.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed.", ex);
        }
    }
}
