using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Application.Platform.Services;

namespace ModularSaaS.Application.Platform;

public static class PlatformModule
{
    public static IServiceCollection AddPlatformModule(this IServiceCollection services)
    {
        services.AddScoped<IPlatformAuthService, PlatformAuthService>();
        services.AddScoped<IPlatformUserService, PlatformUserService>();
        services.AddValidatorsFromAssembly(typeof(PlatformModule).Assembly, includeInternalTypes: true);

        return services;
    }
}
