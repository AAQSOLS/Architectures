using Asp.Versioning;
using ModularSaaS.Api.Transformers;
using ModularSaaS.Application.Shared.Constants;
using Scalar.AspNetCore;

namespace ModularSaaS.Api.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddApiVersioningAndOpenApi(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(ApiVersionConstants.MajorV1, ApiVersionConstants.MinorV0);
            options.ReportApiVersions = true;

            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader(ApiVersionConstants.HeaderName),
                new QueryStringApiVersionReader(ApiVersionConstants.QueryParameterName));
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        })
        .AddOpenApi(options =>
        {
            options.Document.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.Document.AddOperationTransformer<TenantHeaderOperationTransformer>();
        });

        return services;
    }

    public static WebApplication MapOpenApiAndScalar(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().WithDocumentPerVersion();

            app.MapScalarApiReference(options =>
            {
                options.WithTitle(AppOpenApiConstants.Title)
                       .WithTheme(ScalarTheme.Moon)
                       .WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
            });
        }

        return app;
    }
}
