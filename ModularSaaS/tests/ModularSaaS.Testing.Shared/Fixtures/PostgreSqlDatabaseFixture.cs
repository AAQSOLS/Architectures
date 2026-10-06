using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application;
using ModularSaaS.Infrastructure;
using ModularSaaS.Infrastructure.Persistence;
using ModularSaaS.Observability;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace ModularSaaS.Testing.Shared.Fixtures;

public sealed class PostgreSqlDatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string? _databaseName;
    private bool _isLocalInstance;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var customConn = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(customConn))
        {
            ConnectionString = customConn;
            await MigrateAndSeedDatabaseAsync();
            return;
        }

        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:17-alpine")
                .WithDatabase("ModularSaaS_Test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
        }
        catch
        {
            // Fallback for developer environments without Docker Desktop
            _isLocalInstance = true;
            _databaseName = $"modularsaas_test_{Guid.NewGuid():N}";
            var adminConnection = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres;Include Error Detail=true;";
            try
            {
                await using var conn = new NpgsqlConnection(adminConnection);
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE \"{_databaseName}\";";
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // Local PostgreSQL server might not be running or credentials differ
            }

            ConnectionString = $"Host=localhost;Port=5432;Database={_databaseName};Username=postgres;Password=postgres;Include Error Detail=true;";
        }

        await MigrateAndSeedDatabaseAsync();
    }

    private async Task MigrateAndSeedDatabaseAsync()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:Secret"] = "DevelopmentOnlyLocalSigningKey_MustBeReplacedInProductionMin32Bytes!",
                ["Jwt:Issuer"] = "ModularSaaS",
                ["Jwt:Audience"] = "ModularSaaS.Client",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7",
                ["Seed:PlatformAdminEmail"] = "superadmin@modularsaas.local",
                ["Seed:PlatformAdminPassword"] = "P@ssword123!",
                ["Seed:PlatformAdminFirstName"] = "Platform",
                ["Seed:PlatformAdminLastName"] = "Admin",
                ["Seed:DefaultTenantName"] = "Default Organization",
                ["Seed:DefaultTenantIdentifier"] = "default",
                ["Seed:DefaultTenantAdminEmail"] = "admin@default.local",
                ["Seed:DefaultTenantAdminPassword"] = "P@ssword123!",
                ["Seed:DefaultTenantAdminFirstName"] = "Default",
                ["Seed:DefaultTenantAdminLastName"] = "Admin",
                ["Outbox:PollingIntervalSeconds"] = "60",
                ["Outbox:BatchSize"] = "20",
                ["Outbox:MaxRetryCount"] = "3",
                ["Observability:ServiceName"] = "ModularSaaS.Tests",
                ["Observability:Exporter"] = "Console"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddObservability(configuration);

        var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.ApplyMigrationsAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
        else if (_isLocalInstance && !string.IsNullOrWhiteSpace(_databaseName))
        {
            try
            {
                var adminConnection = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres;Include Error Detail=true;";
                await using var connection = new NpgsqlConnection(adminConnection);
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE);";
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // Ignore teardown errors in local instance
            }
        }
    }
}
