using Mapster;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class MappingTests
{
    [Fact]
    public void Application_mappings_compile_successfully()
    {
        var config = new TypeAdapterConfig();
        config.RequireExplicitMapping = true;
        config.RequireDestinationMemberSource = true;
        config.Scan(typeof(ModularSaaS.Application.DependencyInjection).Assembly);
        config.Compile();
    }

    [Fact]
    public void Host_mappings_compile_successfully()
    {
        var config = new TypeAdapterConfig();
        config.RequireExplicitMapping = true;
        config.RequireDestinationMemberSource = true;
        config.Scan(
            typeof(ModularSaaS.Application.DependencyInjection).Assembly,
            typeof(ModularSaaS.Api.Program).Assembly);
        config.Compile();
    }
}
