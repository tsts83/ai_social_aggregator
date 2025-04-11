namespace SocialAggregatorAPI.Controllers;

using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

[ApiController]
[Route("api/config")]
[Authorize(Policy = "ApiKeyOrJwt")]

public class ConfigController : ControllerBase
{
    private readonly IDbConnection _dbConnection;
    private readonly IAppConfigService _appConfigService;

    public ConfigController(IConfiguration configuration, IAppConfigService appConfigService)
    {
        _dbConnection = new MySqlConnection(configuration.GetConnectionString("DefaultConnection"));
        _appConfigService = appConfigService ?? throw new ArgumentNullException(nameof(appConfigService));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateConfig([FromBody] AppConfigRow request)
    {
        var sql = "UPDATE AppConfig SET `Value` = @Value WHERE `Section` = @Section AND `Key` = @Key";

        try
        {
            var result = await _dbConnection.ExecuteAsync(sql, new
            {
                request.Section,
                request.Key,
                request.Value
            });

            if (result == 0)

            {
                return NotFound("No matching config found to update.");
            }

            if (result > 0)
            {
                await _appConfigService.RefreshConfigAsync();
                return Ok();
            }
        }
        catch (MySqlException ex)
        {
            return BadRequest($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return BadRequest($"An error occurred: {ex.Message}");
        }


        return NotFound();
    }
}
