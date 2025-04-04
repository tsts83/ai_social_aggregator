using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SocialAggregatorAPI;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Helpers;
using SocialAggregatorAPI.Models;
using System.Text;

public partial class Program
{
    private static void Main(string[] args)
    {
        // Initialize Serilog for logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Create the WebApplication builder and use Serilog
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        // Register application services
        ConfigureServices(builder);

        // Build the application
        var app = builder.Build();

        // Configure the middleware pipeline
        ConfigureMiddleware(app);

        // Run the application
        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Register JwtReader as Singleton
        services.AddSingleton<JwtReader>();

        // Add authentication and authorization
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var serviceProvider = builder.Services.BuildServiceProvider();
                var jwtReader = serviceProvider.GetRequiredService<JwtReader>();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtReader.GetIssuer(),
                    ValidAudience = jwtReader.GetAudience(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtReader.GetKey()))
                };
            });

        services.AddAuthorization();
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Add Swagger configuration
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Social Aggregator API", Version = "v1" });

            // Configure Swagger to use JWT authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer {token}' to authenticate"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Configure the database connection
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
            )
        );

        // Load newsaggregation.json into the builder's configuration
        builder.Configuration.AddJsonFile("newsaggregation.json", optional: false, reloadOnChange: true);

        // Bind to settings class
        services.Configure<NewsAggregationSettings>(builder.Configuration.GetSection("NewsAggregation"));

        // Register the config service as a singleton
        services.AddSingleton<NewsAggregationConfigReader>();

        // Register IHttpClientFactory
        services.AddHttpClient();

        // Register NewsFetcherService as a hosted service
        services.AddHostedService<NewsFetcherService>();

        builder.Services.AddHttpClient<IContentFetcher, NewsApiFetcher>();
        // Register the NewsApiFetcher
        services.AddSingleton<NewsApiFetcher>();}

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Aggregator API v1"));
        }

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
