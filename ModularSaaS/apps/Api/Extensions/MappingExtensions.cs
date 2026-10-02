using Mapster;
using MapsterMapper;

namespace ModularSaaS.Api.Extensions;

public static class MappingExtensions
{
    public static IServiceCollection AddAppMapping(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.RequireExplicitMapping = true;
        config.RequireDestinationMemberSource = true;
        config.Scan(
            typeof(ModularSaaS.Application.DependencyInjection).Assembly,
            typeof(Program).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
