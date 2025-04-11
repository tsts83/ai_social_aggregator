namespace SocialAggregatorAPI;

using System.Data;
using Dapper;
using MySqlConnector;

public class AppConfigRepository : IAppConfigRepository
{
    private readonly IDbConnection _dbConnection;

    public AppConfigRepository(IConfiguration configuration)
    {
        _dbConnection = new MySqlConnection(configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<IEnumerable<AppConfig>> GetAllConfigsAsync()
    {
        var sql = "SELECT Section, `Key`, `Value`, CreatedAt, UpdatedAt FROM AppConfig";
        return await _dbConnection.QueryAsync<AppConfig>(sql);
    }

}
