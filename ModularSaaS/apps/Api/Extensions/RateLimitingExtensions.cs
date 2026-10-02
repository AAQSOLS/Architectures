using System.Threading.RateLimiting;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicyName = RateLimitPolicies.AuthStrict;
    public const string GeneralPolicyName = RateLimitPolicies.General;

    private const string AnonymousKey = "anonymous";
    private const string AuthenticatedKey = "authenticated";
    private const string UnknownClientIp = "unknown";

    public static IServiceCollection AddRateLimitingInternal(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthPolicyName, httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? UnknownClientIp;
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = RateLimitPolicies.AuthStrictPermitLimit,
                        Window = TimeSpan.FromMinutes(RateLimitPolicies.AuthStrictWindowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = RateLimitPolicies.AuthStrictQueueLimit
                    });
            });

            options.AddPolicy(GeneralPolicyName, httpContext =>
            {
                var key = httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? AuthenticatedKey
                    : httpContext.Connection.RemoteIpAddress?.ToString() ?? AnonymousKey;

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = RateLimitPolicies.GeneralPermitLimit,
                        Window = TimeSpan.FromMinutes(RateLimitPolicies.GeneralWindowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = RateLimitPolicies.GeneralQueueLimit
                    });
            });
        });

        return services;
    }
}
