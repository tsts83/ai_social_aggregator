using Microsoft.EntityFrameworkCore;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;

namespace SocialAggregatorAPI
{
    public class NewsFetcherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NewsFetcherService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NewsApiFetcher _newsApiFetcher;
        private readonly IAppConfigService _appConfigService;
        public NewsFetcherService(IServiceProvider serviceProvider, ILogger<NewsFetcherService> logger,
                                  IHttpClientFactory httpClientFactory,
                                  IAppConfigService appConfigService,
                                   NewsApiFetcher newsApiFetcher)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _newsApiFetcher = newsApiFetcher;
            _appConfigService = appConfigService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var settings = await _appConfigService.GetConfigAsync();

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

                var fetchInterval = settings.NewsAggregation?.FetchIntervalMinutes ?? 60;
                await Task.Delay(TimeSpan.FromMinutes(fetchInterval), stoppingToken);
            }
        }

        public async Task FetchAndStoreNews()
        {
            var settings = await _appConfigService.GetConfigAsync();

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Check if there are more than 5 unposted articles
            var unpostedCount = await dbContext.NewsArticles
                .Where(a => !a.IsPosted)
                .CountAsync();

            if (unpostedCount >= 5)
            {
                _logger.LogInformation("There are {Count} unposted articles. Skipping news fetch.", unpostedCount);
                return; // Skip the fetching process if there are 5 or more unposted articles
            }

            _logger.LogInformation("Fetching and storing news...");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MySocialAggregatorBot/1.0");

            if (settings.NewsAggregation?.SourcePreferences?.NewsDataAPI == true)
            {
                await _newsApiFetcher.FetchNewsDataApiNews(dbContext, httpClient);
            }

            await dbContext.SaveChangesAsync();
        }

    }
}
