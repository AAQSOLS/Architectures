using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.AspNetCore.Authorization;
using ModularSaaS.Security.AspNetCore.Builder;
using ModularSaaS.Security.AspNetCore.Identity;
using ModularSaaS.Security.AspNetCore.Middleware;
using ModularSaaS.Security.AspNetCore.Options;
using ModularSaaS.Security.Core.Events;
using ModularSaaS.Security.Core.Hashing;
using ModularSaaS.Security.Core.Tokens;

namespace ModularSaaS.Security.AspNetCore;

public static class DependencyInjection
{
    public static IServiceCollection AddSecurityPlugin(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SecurityPluginBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Options registration with startup validation
        var securitySection = configuration.GetSection(SecurityOptions.SectionName);
        var jwtSection = securitySection.GetSection("Jwt");
        var secret = jwtSection["SigningKey"] ?? jwtSection["Secret"] ?? string.Empty;
        var issuer = jwtSection["Issuer"] ?? string.Empty;
        var audience = jwtSection["Audience"] ?? string.Empty;

        // 2. Default cryptographic & security services
        services.TryAddSingleton<IPasswordHasher>(_ => new BCryptPasswordHasher());
        services.TryAddSingleton<ISecureRandomGenerator, SecureRandomGenerator>();
        services.TryAddSingleton<ITokenRevocationRegistry, InMemoryTokenRevocationRegistry>();
        services.TryAddSingleton<ISecurityEventSink, DefaultSecurityEventSink>();
        services.TryAddSingleton(TimeProvider.System);

        // 3. Ambient and HTTP identity context
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUser, HttpCurrentUser>();
        services.TryAddScoped<IImpersonationContext, HttpImpersonationContext>();
        services.TryAddSingleton<ISecurityContextAccessor, AmbientSecurityContextAccessor>();

        // 4. Token service
        if (!string.IsNullOrWhiteSpace(secret))
        {
            var jwtServiceOptions = new JwtServiceOptions
            {
                Issuer = issuer,
                Audience = audience,
                SigningKey = secret,
                ClockSkew = TimeSpan.Zero
            };

            services.TryAddSingleton<ITokenService>(sp =>
                new JsonWebTokenService(jwtServiceOptions, sp.GetService<TimeProvider>()));

            // 5. JWT Bearer authentication handler with zero clock skew
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        // 6. Dynamic permission policy authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // 7. Invoke consumer customization builder
        var builder = new SecurityPluginBuilder(services);
        configure?.Invoke(builder);

        return services;
    }

    public static IApplicationBuilder UseSecurityPlugin(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseAuthentication();
        app.UseMiddleware<TenantIsolationGuardMiddleware>();
        app.UseAuthorization();

        return app;
    }
}
