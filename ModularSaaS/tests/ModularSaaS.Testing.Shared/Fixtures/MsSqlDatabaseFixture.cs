using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularSaaS.Application;
using ModularSaaS.Infrastructure;
using ModularSaaS.Infrastructure.Persistence;
using ModularSaaS.Observability;
using Testcontainers.MsSql;
using Xunit;

namespace ModularSaaS.Testing.Shared.Fixtures;

public sealed class MsSqlDatabaseFixture : IAsyncLifetime
{
    private MsSqlContainer? _container;
    private string? _databaseName;
    private bool _isLocalDb;

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
            _container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Strong_Passw0rd!")
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
        }
        catch
        {
            // Fallback for Windows developer environments without Docker Desktop
            _isLocalDb = true;
            _databaseName = $"ModularSaaS_Test_{Guid.NewGuid():N}";
            ConnectionString = $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
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
        else if (_isLocalDb && !string.IsNullOrWhiteSpace(_databaseName))
        {
            try
            {
                var masterConnection = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;TrustServerCertificate=True";
                await using var connection = new SqlConnection(masterConnection);
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = $@"
                    ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{_databaseName}];";
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // Ignore teardown errors in LocalDB
            }
        }
    }
}
