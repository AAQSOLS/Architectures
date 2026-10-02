using ModularSaaS.Api.Middleware;
using ModularSaaS.Observability;
using Serilog;

namespace ModularSaaS.Api.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseObservability();
        app.UseSerilogRequestLogging();
        app.UseRequestLogContextEnrichment();

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseAuthorization();

        return app;
    }
}
