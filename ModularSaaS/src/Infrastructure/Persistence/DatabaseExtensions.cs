using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Infrastructure.Persistence.Seed;

namespace ModularSaaS.Infrastructure.Persistence;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await seeder.InitializeAsync();
    }
}
