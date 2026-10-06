using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using ModularSaaS.Api.Common;
using ModularSaaS.Application.Shared.Constants;

namespace ModularSaaS.Api.Transformers;

internal sealed class TenantHeaderOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var path = context.Description.RelativePath ?? string.Empty;
        if (!path.Contains(ApiRoutes.PlatformSegment, StringComparison.OrdinalIgnoreCase))
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = AppHeaders.TenantId,
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
                Description = "Target Tenant GUID (optional if passed via JWT claim)"
            });
        }

        return Task.CompletedTask;
    }
}
