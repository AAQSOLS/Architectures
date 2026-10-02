using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Identity.Services;

namespace ModularSaaS.Application.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<IUserService>(sp => sp.GetRequiredService<UserService>());
        services.AddScoped<IUserRegistrationService>(sp => sp.GetRequiredService<UserService>());
        services.AddScoped<IUserImpersonationService>(sp => sp.GetRequiredService<UserService>());
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddValidatorsFromAssembly(typeof(IdentityModule).Assembly, includeInternalTypes: true);

        return services;
    }
}
