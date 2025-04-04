using Microsoft.Extensions.Options;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;

namespace SocialAggregatorAPI
{
    public class NewsFetcherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NewsFetcherService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NewsAggregationSettings _settings;
        private readonly IConfiguration _config;
        private readonly NewsApiFetcher _newsApiFetcher;


        public NewsFetcherService(IServiceProvider serviceProvider, ILogger<NewsFetcherService> logger, 
                                  IHttpClientFactory httpClientFactory, IOptions<NewsAggregationSettings> settings, 
                                  IConfiguration config, NewsApiFetcher newsApiFetcher)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _config = config;
            _newsApiFetcher = newsApiFetcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("News Fetcher Service is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FetchAndStoreNews();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching news.");
                }

                await Task.Delay(TimeSpan.FromMinutes(_settings.FetchIntervalMinutes), stoppingToken);
            }
        }

        public async Task FetchAndStoreNews()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MySocialAggregatorBot/1.0");

            if (_settings.SourcePreferences.NewsDataAPI)
                await _newsApiFetcher.FetchNewsDataApiNews(dbContext, httpClient);

            await dbContext.SaveChangesAsync();
        }
    }
}
