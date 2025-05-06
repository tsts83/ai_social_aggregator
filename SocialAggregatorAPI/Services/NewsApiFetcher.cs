using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;

namespace SocialAggregatorAPI;

public class NewsApiFetcher : IContentFetcher
{
    private readonly IConfiguration _config;
    private readonly ILogger<NewsApiFetcher> _logger;
    private readonly IAppConfigService _appConfigService;

    public NewsApiFetcher(IAppConfigService appConfigService, IConfiguration config, ILogger<NewsApiFetcher> logger)
    {
        _appConfigService = appConfigService;
        _config = config;
        _logger = logger;
    }

    public async Task<AppDbContext> FetchNewsDataApiNews(AppDbContext dbContext, HttpClient httpClient)
    {
        var settings = await _appConfigService.GetConfigAsync();
        bool useKafka = _config.GetValue<bool>("UseKafka");

        if (useKafka)
        {
            return await ConsumeFromKafka(dbContext);
        }
        else
        {
            return await FetchFromRestApi(dbContext, httpClient, settings);
        }
    }

    private async Task<AppDbContext> FetchFromRestApi(AppDbContext dbContext, HttpClient httpClient, AppSettings settings)
    {
        _logger.LogInformation("🌐 Fetching news from NewsData.io via REST API...");

        string? apiKey = _config["NewsDataApiKey"] ?? Environment.GetEnvironmentVariable("NEWSDATA_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("API key for NewsData.io is not configured.");

        string language = settings.NewsAggregation?.Filters?.Language ?? "en";
        var keywords = settings.NewsAggregation?.Filters?.Keywords;
        string query = string.Join(" AND ", keywords ?? new List<string>());
        string encodedQuery = Uri.EscapeDataString(query);
        string requestUrl = $"https://newsdata.io/api/1/latest?apikey={apiKey}&q={encodedQuery}&language={language}";

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
        
        var articles = newsApiResponse?.Results?.Select(result => new NewsArticle
        {
            Title = result.Title,
            Content = result.Description,
            Source = "NewsDataAPI",
            Url = result.SourceUrl,
            ThumbnailUrl = result.ImageUrl,
            PublishedAt = DateTime.TryParse(result.PubDate, out var parsed) ? parsed : DateTime.UtcNow
        }) ?? new List<NewsArticle>();

        return await ProcessArticles(dbContext, articles);
    }

    private async Task<AppDbContext> ConsumeFromKafka(AppDbContext dbContext)
    {
        _logger.LogInformation("📡 Starting Kafka consumption...");

        var kafkaSettings = _config.GetSection("Kafka");
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings["BootstrapServers"],
            GroupId = kafkaSettings["GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(kafkaSettings["Topic"]);
        _logger.LogInformation($"✅ Subscribed to Kafka topic: {kafkaSettings["Topic"]}");

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var articles = new List<NewsArticle>();
        int messageCount = 0;

        try
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogInformation("⏳ Waiting for Kafka message...");

                var result = consumer.Consume(cancellationTokenSource.Token);

                if (result == null)
                {
                    _logger.LogWarning("⚠️ No message received.");
                    continue;
                }

                _logger.LogInformation($"📥 Message received from Kafka: {result.Message.Value}");

                var item = JsonSerializer.Deserialize<KafkaNewsMessage>(
                    result.Message.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (item != null &&
                    !string.IsNullOrEmpty(item.Title) &&
                    !string.IsNullOrEmpty(item.Link) &&
                    !string.IsNullOrEmpty(item.Description))
                {
                    var article = new NewsArticle
                    {
                        Title = item.Title,
                        Content = item.Description,
                        ThumbnailUrl = item.Image_url,
                        Source = "NewsDataAPI",
                        Url = item.Link,
                        PublishedAt = DateTime.TryParse(item.PubDate, out var parsed) ? parsed : DateTime.UtcNow
                    };

                    articles.Add(article);
                    messageCount++;

                    _logger.LogInformation($"✅ Added article {messageCount}: {article.Title}");

                    // Keep only the latest 10
                    if (articles.Count > 10)
                    {
                        var removed = articles[0].Title;
                        articles.RemoveAt(0);
                        _logger.LogInformation($"♻️ Removed oldest article to keep list size: {removed}");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Skipping invalid or incomplete Kafka message.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("⏹️ Kafka consume timed out after 10 seconds.");
        }
        catch (ConsumeException ex)
        {
            _logger.LogError($"❌ Kafka consume error: {ex.Message}");
        }
        finally
        {
            _logger.LogInformation("📴 Closing Kafka consumer.");
            consumer.Close();
        }

        _logger.LogInformation($"📦 Total articles fetched from Kafka: {articles.Count}");
        return await ProcessArticles(dbContext, articles);
    }



    private async Task<AppDbContext> ProcessArticles(AppDbContext dbContext, IEnumerable<NewsArticle> articles)
    {
        foreach (var article in articles)
        {
            // Check if content is null or empty
            if (string.IsNullOrEmpty(article.Content))
            {
                _logger.LogInformation($"Skipping article with title '{article.Title}' because description is null or empty.");
                continue;  // Skip this article and move to the next
            }

            // Check if ImageUrl is null or empty
            if (string.IsNullOrEmpty(article.ThumbnailUrl))
            {
                _logger.LogInformation($"Skipping article with title '{article.Title}' because image URL is null or empty.");
                continue;  // Skip this article and move to the next
            }

            // Check if article already exists in the database
            var existingTitles = await dbContext.NewsArticles
                .Select(article => article.Title)
                .ToListAsync();

            var isDuplicate = existingTitles.Any(existing =>
                article.Title.Contains(existing, StringComparison.OrdinalIgnoreCase) ||
                existing.Contains(article.Title, StringComparison.OrdinalIgnoreCase));

            if (!isDuplicate)
            {
                dbContext.NewsArticles.Add(article);
                existingTitles.Add(article.Title); // update memory cache
                _logger.LogInformation($"✅ Added article from {article.Source}: {article.Title}");
            }
            else
            {
                _logger.LogInformation($"ℹ️ Skipped duplicate (fuzzy match): {article.Title}");
            }
        }

        return dbContext;
    }
}
