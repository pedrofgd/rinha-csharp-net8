using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace IntegrationTests.Helpers;

public static class TestDatabaseContext
{
    public static async Task ClearDatabase(TestWebApplicationFactory<Program> factory)
    {
        var connection = factory.Services.GetRequiredService<NpgsqlConnection>();
        
        await connection.OpenAsync();
        
        await using var command = new NpgsqlCommand("TRUNCATE TABLE pessoas CASCADE;", connection);
        await command.ExecuteNonQueryAsync();

        await connection.CloseAsync();
        await connection.DisposeAsync();
    }
}