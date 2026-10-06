using Npgsql;
using Respawn;

namespace ModularSaaS.Testing.Shared.Fixtures;

public sealed class DatabaseResetter
{
    private Respawner? _respawner;

    public async Task ResetAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"],
            SchemasToInclude = ["public"]
        });

        await _respawner.ResetAsync(connection);
    }
}
