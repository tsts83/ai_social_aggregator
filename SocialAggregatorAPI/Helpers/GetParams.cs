using Serilog;

public static class GetParams
{
    public static string GetConnectionString(IConfiguration configuration)
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
        return connectionString ?? string.Empty;
    }
}