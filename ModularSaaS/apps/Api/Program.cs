using ModularSaaS.Api.Extensions;
using ModularSaaS.Application;
using ModularSaaS.Infrastructure;
using ModularSaaS.Observability;

var builder = WebApplication.CreateBuilder(args);

// 1. Host Infrastructure & Logging
builder.Host.UseAppSerilog();

// 2. Service Registrations (Each concern owns its own configuration)
builder.Services.AddControllers();
builder.Services
    .AddAppCors()
    .AddAppMapping()
    .AddAppAuthorization()
    .AddAppHealthChecks()
    .AddRateLimitingInternal()
    .AddApiVersioningAndOpenApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddObservability(builder.Configuration);

var app = builder.Build();

// 3. Request Middleware Pipeline
app.UseAppMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApiAndScalar();
    await ModularSaaS.Infrastructure.Persistence.DatabaseExtensions.ApplyMigrationsAsync(app.Services);
}

app.MapControllers();
app.MapAppHealthChecks();

await app.RunAsync();

// Expose Program for test scaffolding
namespace ModularSaaS.Api
{
    public partial class Program;
}
