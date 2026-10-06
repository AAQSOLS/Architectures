using Microsoft.Data.SqlClient;
using Respawn;

namespace ModularSaaS.Testing.Shared.Fixtures;

public sealed class DatabaseResetter
{
    private Respawner? _respawner;

    public async Task ResetAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore = ["__EFMigrationsHistory"],
            SchemasToInclude = ["dbo"]
        });

        await _respawner.ResetAsync(connection);
    }
}
