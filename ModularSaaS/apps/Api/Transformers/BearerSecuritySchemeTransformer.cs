using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Transformers;

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = AppHeaders.Authorization,
            Type = SecuritySchemeType.Http,
            Scheme = AppOpenApiConstants.BearerSecurityScheme.ToLowerInvariant(),
            BearerFormat = AppOpenApiConstants.BearerFormat,
            In = ParameterLocation.Header,
            Description = AppOpenApiConstants.BearerDescription
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[AppOpenApiConstants.BearerSecurityScheme] = securityScheme;

        document.Security ??= [];
        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(AppOpenApiConstants.BearerSecurityScheme, document)] = []
        };
        document.Security.Add(requirement);

        return Task.CompletedTask;
    }
}
