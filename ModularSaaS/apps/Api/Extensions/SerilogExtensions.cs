using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ModularSaaS.Application.Shared.Constants;
using Serilog;
using Serilog.Context;

namespace ModularSaaS.Api.Extensions;

public static class SerilogExtensions
{
    private const string NoneValue = "none";
    private const string AnonymousValue = "anonymous";
    private const string TenantIdProperty = "TenantId";
    private const string UserIdProperty = "UserId";
    private const string ImpersonatedByProperty = "ImpersonatedBy";

    public static ConfigureHostBuilder UseAppSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration)
                         .Enrich.FromLogContext()
                         .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

        return host;
    }

    public static IApplicationBuilder UseRequestLogContextEnrichment(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var tenantId = context.Request.Headers[AppHeaders.TenantId].FirstOrDefault()
                ?? context.User.FindFirst(AppClaimTypes.TenantId)?.Value
                ?? NoneValue;

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? AnonymousValue;

            var act = context.User.FindFirst(AppClaimTypes.Actor)?.Value;

            using (LogContext.PushProperty(TenantIdProperty, tenantId))
            using (LogContext.PushProperty(UserIdProperty, userId))
            using (LogContext.PushProperty(ImpersonatedByProperty, act ?? NoneValue))
            {
                await next();
            }
        });
    }
}
