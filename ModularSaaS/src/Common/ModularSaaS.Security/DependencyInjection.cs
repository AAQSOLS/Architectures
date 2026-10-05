using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Security.Authorization;
using ModularSaaS.Security.Cryptography;

namespace ModularSaaS.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddAppSecurity(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<ISecureRandomGenerator, SecureRandomGenerator>();
        services.AddSingleton<BCryptPasswordHasher>();

        return services;
    }
}
