using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ModularSaaS.Application.Identity.Abstractions;
using ModularSaaS.Application.Platform.Abstractions;
using ModularSaaS.Application.Shared.Abstractions;
using ModularSaaS.Application.Shared.Constants;
using ModularSaaS.Application.Tenancy.Abstractions;
using ModularSaaS.Infrastructure.Caching;
using ModularSaaS.Infrastructure.Communications;
using ModularSaaS.Infrastructure.Persistence;
using ModularSaaS.Infrastructure.Persistence.Interceptors;
using ModularSaaS.Infrastructure.Persistence.Outbox;
using ModularSaaS.Security.Cryptography;
using ModularSaaS.Infrastructure.Persistence.Repositories.Identity;
using ModularSaaS.Infrastructure.Persistence.Repositories.Platform;
using ModularSaaS.Infrastructure.Persistence.Repositories.Tenancy;
using ModularSaaS.Infrastructure.Persistence.Seed;
using ModularSaaS.Infrastructure.Security;
using ModularSaaS.Infrastructure.Tenancy;
using ModularSaaS.Infrastructure.Time;

namespace ModularSaaS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        // 1. Strongly-typed configuration options with startup validation
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{JwtOptions.SectionName}' is missing.");
        Validator.ValidateObject(jwtOptions, new ValidationContext(jwtOptions), validateAllProperties: true);

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SeedOptions>()
            .BindConfiguration(SeedOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OutboxProcessorOptions>()
            .BindConfiguration(OutboxProcessorOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 2. Core Infrastructure Services
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<BCryptPasswordHasher>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddScoped<JwtTokenService>();
        services.AddScoped<ITokenService>(sp => sp.GetRequiredService<JwtTokenService>());
        services.AddScoped<ITenantTokenService>(sp => sp.GetRequiredService<JwtTokenService>());
        services.AddScoped<IPlatformTokenService>(sp => sp.GetRequiredService<JwtTokenService>());
        services.AddScoped<ISecureTokenGenerator>(sp => sp.GetRequiredService<JwtTokenService>());

        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
        services.AddScoped<ModularSaaS.Security.ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantSetter>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantLookupService, CachedTenantLookupService>();

        services.AddSingleton<PermissionCache>();
        services.AddSingleton<IPermissionCache>(sp => sp.GetRequiredService<PermissionCache>());
        services.AddSingleton<IPermissionCacheReader>(sp => sp.GetRequiredService<PermissionCache>());
        services.AddSingleton<IPermissionCacheInvalidator>(sp => sp.GetRequiredService<PermissionCache>());

        services.AddScoped<ModularSaaS.Security.Authorization.IPermissionEvaluator, PermissionEvaluator>();

        services.AddTransient<IEmailSender, ConsoleEmailSender>();

        // 3. Interceptors
        services.AddScoped<TenantInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<OutboxInterceptor>();

        // 4. Database persistence
        var connectionString = configuration.GetConnectionString(DatabaseOptions.DefaultConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{DatabaseOptions.DefaultConnectionName}' is not configured.");
        }

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            options.AddInterceptors(
                sp.GetRequiredService<TenantInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<OutboxInterceptor>());
        });

        // 5. Repositories & Unit of Work
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPlatformUserRepository, PlatformUserRepository>();

        services.AddScoped<UserRepository>();
        services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());
        services.AddScoped<IUserPermissionReader>(sp => sp.GetRequiredService<UserRepository>());

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbInitializer>();

        // 6. Outbox background processing & event handlers
        services.AddHostedService<OutboxProcessorBackgroundService>();
        services.ScanDomainEventHandlers(typeof(IDomainEventHandler<>).Assembly);

        // 7. Health Checks
        services.AddHealthChecks()
            .AddCheck<ModularSaaS.Infrastructure.Health.DatabaseHealthCheck>("database", tags: [HealthCheckTags.Ready]);

        // 8. Authentication with validated JWT parameters
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    private static IServiceCollection ScanDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        var openHandlerType = typeof(IDomainEventHandler<>);
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openHandlerType)
                .Select(i => new { ServiceType = i, ImplementationType = t }));

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.ServiceType, handler.ImplementationType);
        }

        return services;
    }
}
