namespace SocialAggregatorAPI;

using System.Data;
using Dapper;
using MySqlConnector;
using Serilog;

public class AppConfigRepository : IAppConfigRepository
{
    private readonly IDbConnection _dbConnection;

    public AppConfigRepository(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
            Log.Information("Using connection string from env var: {ConnectionString}", connectionString?.Replace("Password=", "Password=*****"));
        }
        else
        {
            Log.Information("Using connection string from config: {ConnectionString}", connectionString?.Replace("Password=", "Password=*****"));
        }
        _dbConnection = new MySqlConnection(connectionString);
    }

    public async Task<IEnumerable<AppConfig>> GetAllConfigsAsync()
    {
        var sql = "SELECT Section, `Key`, `Value`, CreatedAt, UpdatedAt FROM AppConfig";
        return await _dbConnection.QueryAsync<AppConfig>(sql);
    }

}
