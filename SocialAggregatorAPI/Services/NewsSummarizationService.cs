namespace SocialAggregatorAPI;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;

public class NewsSummarizationService : BackgroundService
{
    private readonly ILogger<NewsSummarizationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IAppConfigService _appConfigService;

    public NewsSummarizationService(
        ILogger<NewsSummarizationService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IAppConfigService appConfigService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _httpClient = new HttpClient();
        _appConfigService = appConfigService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NewsSummarizationService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var unsummarizedArticles = await dbContext.NewsArticles
                .Where(a => !a.IsProcessed && !string.IsNullOrEmpty(a.Content))
                .ToListAsync(stoppingToken);

            foreach (var article in unsummarizedArticles)
            {
                if (string.IsNullOrEmpty(article.Title))
                {
                    _logger.LogWarning($"Article with empty or null title cannot be summarized.");
                    continue;
                }

                if (article.Content == null)
                {
                    _logger.LogWarning($"Article '{article.Title}' has null content and cannot be summarized.");
                    continue;
                }

                var summary = await GenerateFunnySummaryAsync(article.Title, article.Content);
                if (!string.IsNullOrEmpty(summary))
                {
                    article.AiSummary = summary;
                    article.IsProcessed = true;

                    _logger.LogInformation($"Summarized article '{article.Title}'");
                }
                else
                {
                    _logger.LogWarning($"Failed to summarize article '{article.Title}'");
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);

            // Wait before the next check
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }

        _logger.LogInformation("NewsSummarizationService stopped.");
    }

    private async Task<string?> GenerateFunnySummaryAsync(string title, string content)
    {   
        var settings = await _appConfigService.GetConfigAsync();
        string apiKey = _configuration["HuggingFaceApiKey"] ?? Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY");
        if (string.IsNullOrEmpty(apiKey)) return null;

        var prompt = settings.AiPrompt
            .Replace("{title}", title)
            .Replace("{content}", content);

        var requestBody = new
        {
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            model = settings.AiMode,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://router.huggingface.co/novita/v3/openai/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to get summary: {response.StatusCode}");
            return null;
        }

        var responseString = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            var responeContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return responeContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse summary response.");
            return null;
        }
    }
}
