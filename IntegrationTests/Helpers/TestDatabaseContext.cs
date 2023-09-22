using Npgsql;
using WebEndpoints;

namespace IntegrationTests.Helpers;

public static class TestDatabaseContext
{
    public static async Task ClearDatabase()
    {
        const string truncate = "TRUNCATE TABLE pessoas cascade;";
        await using var connection = await ConnectionFactory.GetPostgresConnection();
        await using var command = new NpgsqlCommand(truncate, connection);
        await command.ExecuteNonQueryAsync();
    }
}