using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Api.Common;
using ModularSaaS.Api.Controllers;
using ModularSaaS.Api.Extensions;
using Xunit;

namespace ModularSaaS.Architecture.Tests.Rules;

public class OpenApiVersioningTests
{
    private static readonly Assembly ApiAssembly = typeof(BaseApiController).Assembly;

    [Fact]
    public void All_Api_Controllers_Must_Inherit_BaseApiController_And_Have_ApiVersion()
    {
        var controllerTypes = ApiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseApiController).IsAssignableFrom(t))
            .ToList();

        Assert.NotEmpty(controllerTypes);

        foreach (var controller in controllerTypes)
        {
            var hasApiVersion = controller.GetCustomAttributes(typeof(ApiVersionAttribute), inherit: false).Length > 0;
            Assert.True(hasApiVersion, $"Controller '{controller.FullName}' must have an [ApiVersion] attribute.");
        }
    }

    [Fact]
    public void All_Api_Controllers_Must_Use_Versioned_Route_Prefix()
    {
        var controllerTypes = ApiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseApiController).IsAssignableFrom(t))
            .ToList();

        foreach (var controller in controllerTypes)
        {
            var routeAttr = controller.GetCustomAttribute<RouteAttribute>(inherit: false);
            Assert.NotNull(routeAttr);
            Assert.Contains(ApiRoutes.VersionPrefix, routeAttr.Template, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Api_Assembly_Must_Not_Reference_Swashbuckle()
    {
        var referencedAssemblies = ApiAssembly.GetReferencedAssemblies();
        var swashbuckleRef = referencedAssemblies
            .FirstOrDefault(a => a.Name?.Contains("Swashbuckle", StringComparison.OrdinalIgnoreCase) == true);

        Assert.Null(swashbuckleRef);
    }

    [Fact]
    public void Api_Versioning_And_OpenApi_Services_Resolve_Correctly()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        services.AddApiVersioningAndOpenApi();

        var provider = services.BuildServiceProvider();
        var versioningOptions = provider.GetService<Microsoft.Extensions.Options.IOptions<ApiVersioningOptions>>();

        Assert.NotNull(versioningOptions);
        Assert.True(versioningOptions.Value.ReportApiVersions);
    }
}
