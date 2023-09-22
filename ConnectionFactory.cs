using Npgsql;

namespace csharp_net8;

public static class ConnectionFactory
{
    private static string? _postgresConnectionString;

    public static void InitPostgres(string? connectionString)
    {
        _postgresConnectionString = connectionString;
        ArgumentException.ThrowIfNullOrWhiteSpace(_postgresConnectionString);
    }
    
    public static async Task<NpgsqlConnection> GetPostgresConnection()
    {
        var dataSource = NpgsqlDataSource.Create(_postgresConnectionString!);
        return await dataSource.OpenConnectionAsync();
    }
}