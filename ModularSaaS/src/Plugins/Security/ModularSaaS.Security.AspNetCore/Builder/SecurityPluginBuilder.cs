using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularSaaS.Security.Abstractions;
using ModularSaaS.Security.AspNetCore.Authorization;
using ModularSaaS.Security.Core.Hashing;

namespace ModularSaaS.Security.AspNetCore.Builder;

public sealed class SecurityPluginBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    public SecurityPluginBuilder AddPasswordHasher<THasher>()
        where THasher : class, IPasswordHasher
    {
        Services.Replace(ServiceDescriptor.Singleton<IPasswordHasher, THasher>());
        return this;
    }

    public SecurityPluginBuilder AddPasswordHasher(int workFactor = 12)
    {
        Services.Replace(ServiceDescriptor.Singleton<IPasswordHasher>(_ => new BCryptPasswordHasher(workFactor)));
        return this;
    }

    public SecurityPluginBuilder AddTokenRevocationRegistry<TRegistry>()
        where TRegistry : class, ITokenRevocationRegistry
    {
        Services.Replace(ServiceDescriptor.Singleton<ITokenRevocationRegistry, TRegistry>());
        return this;
    }

    public SecurityPluginBuilder AddSecurityEventSink<TSink>()
        where TSink : class, ISecurityEventSink
    {
        Services.Replace(ServiceDescriptor.Singleton<ISecurityEventSink, TSink>());
        return this;
    }

    public SecurityPluginBuilder AddPermissionEvaluator<TEvaluator>()
        where TEvaluator : class, IPermissionEvaluator
    {
        Services.Replace(ServiceDescriptor.Scoped<IPermissionEvaluator, TEvaluator>());
        return this;
    }

    public SecurityPluginBuilder AddDynamicPermissions()
    {
        Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return this;
    }
}
