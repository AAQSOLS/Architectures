namespace ModularSaaS.Application.Shared.Constants;

public static class AppOpenApiConstants
{
    public const string Title = "ModularSaaS API";
    public const string Description = "Enterprise Multi-Tenant SaaS Platform API (.NET 10)";
    public const string BearerSecurityScheme = "Bearer";
    public const string BearerFormat = "JWT";
    public const string BearerDescription = "Enter JWT access token";
    public const string RoutePattern = "/openapi/{documentName}.json";
    public const string ScalarEndpointPrefix = "/scalar";
}
