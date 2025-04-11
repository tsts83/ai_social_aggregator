using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;

namespace SocialAggregatorAPI;

public class NewsApiFetcher : IContentFetcher
{
    private readonly AppSettings _settings;
    private readonly IConfiguration _config;
    private readonly ILogger<NewsApiFetcher> _logger;

    public NewsApiFetcher(IAppConfigService settings, IConfiguration config, ILogger<NewsApiFetcher> logger)
    {
        _settings = settings.CurrentConfig;
        _config = config;
        _logger = logger;
    }

    public async Task<AppDbContext> FetchNewsDataApiNews(AppDbContext dbContext, HttpClient httpClient)
    {
        _logger.LogInformation("Fetching news from NewsData.io...");

        // API Request URL
        string? apiKey = _config["NewsDataApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API key for NewsData.io is not configured.");
        }

        string language = _settings.NewsAggregation?.Filters?.Language ?? "en";  // From newsaggregation.json, default to "en"
        var keywords = _settings.NewsAggregation?.Filters?.Keywords;  // Keywords from newsaggregation.json

        // Build the query string for 'q' by joining keywords with 'AND'
        string query = string.Join(" AND ", keywords);

        // URL encode the query string (important for special characters)
        string encodedQuery = Uri.EscapeDataString(query);
        string requestUrl = $"https://newsdata.io/api/1/latest?apikey={apiKey}&q={encodedQuery}&language={language}";

        // Make HTTP request
        var response = await httpClient.GetStringAsync(requestUrl);

        // Deserialize response
        var newsApiResponse = JsonSerializer.Deserialize<NewsDataResponse>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (newsApiResponse?.Results == null || !newsApiResponse.Results.Any())
        {
            _logger.LogWarning("No articles found in NewsData.io response.");
            return dbContext;
        }

        // Process articles
        foreach (var article in newsApiResponse.Results.Take(_settings.NewsAggregation.MaxArticlesPerFetch))
            {
                // Check if content is null or empty
                if (string.IsNullOrEmpty(article.Description))
                {
                    _logger.LogInformation($"Skipping article with title '{article.Title}' because description is null or empty.");
                    continue;  // Skip this article and move to the next
                }

                // Check if an article with the same title already exists in the database
                var existingArticle = await dbContext.NewsArticles
                    .FirstOrDefaultAsync(a => a.Title == article.Title);

                if (existingArticle == null)
                {
                    // Article doesn't exist, save it to the database
                    dbContext.NewsArticles.Add(new NewsArticle
                    {
                        Title = article.Title,
                        Content = article.Description,
                        Source = "NewsDataAPI",
                        Url = article.SourceUrl,
                        PublishedAt = string.IsNullOrEmpty(article.PubDate) ? DateTime.MinValue : DateTime.Parse(article.PubDate),
                        ThumbnailUrl = article.ImageUrl  // Assuming this is the correct field for the thumbnail
                    });

                    _logger.LogInformation($"Added new article: {article.Title}");
                }
                else
                {
                    _logger.LogInformation($"Article with title '{article.Title}' already exists in the database.");
                }
            }

        _logger.LogInformation("Fetched and stored articles from NewsData.io.");

        return dbContext;
        }
}
