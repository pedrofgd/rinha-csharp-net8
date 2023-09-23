using Npgsql;

namespace WebEndpoints;

public static class ConnectionFactory
{
    private static NpgsqlDataSource _dataSource = null!;

    public static void InitPostgres(string? connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _dataSource = NpgsqlDataSource.Create(connectionString);
        ArgumentNullException.ThrowIfNull(_dataSource);
    }
    
    public static async Task<NpgsqlConnection?> GetPostgresConnection()
    {
        NpgsqlConnection? connection = null;
        while (connection is null)
        {
            try
            {
                connection = await _dataSource.OpenConnectionAsync();
            }
            catch (Exception)
            {
                await Task.Delay(1000);
            }
        }
        
        return connection;
    }
}