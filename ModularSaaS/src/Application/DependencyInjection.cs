using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application.Identity;
using ModularSaaS.Application.Platform;
using ModularSaaS.Application.Tenancy;

namespace ModularSaaS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddPlatformModule();
        services.AddTenancyModule();
        services.AddIdentityModule();

        return services;
    }
}
