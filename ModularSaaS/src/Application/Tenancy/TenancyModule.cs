using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Application.Tenancy.Services;

namespace ModularSaaS.Application.Tenancy;

public static class TenancyModule
{
    public static IServiceCollection AddTenancyModule(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();

        return services;
    }
}
