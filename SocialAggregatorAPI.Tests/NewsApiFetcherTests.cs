namespace SocialAggregatorAPI.Tests;

using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SocialAggregatorAPI;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;
using Microsoft.EntityFrameworkCore;

public class NewsApiFetcherTests
{
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IAppConfigService> _appConfigServiceMock = new();
    private readonly Mock<ILogger<NewsApiFetcher>> _loggerMock = new();

    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options);
    }

    private HttpClient CreateMockHttpClient(object responseObject)
    {
        var messageHandler = new MockHttpMessageHandler(responseObject);
        return new HttpClient(messageHandler);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly object _responseObject;

        public MockHttpMessageHandler(object responseObject)
        {
            _responseObject = responseObject;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(_responseObject);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task FetchNewsDataApiNews_AddsArticlesToDatabase()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "NewsDataApiKey", "test-key" }
        }).Build();

        var settings = new AppSettings
        {
            NewsAggregation = new NewsAggregationConfig
            {
                Filters = new FiltersConfig
                {
                    Language = "en",
                    Keywords = new List<string> { "test", "ai" }
                },
                MaxArticlesPerFetch = 5
            }
        };

        _appConfigServiceMock.Setup(a => a.GetConfigAsync()).ReturnsAsync(settings);

        var newsResponse = new NewsDataResponse
        {
            Results = new List<NewsDataArticle>
            {
                new NewsDataArticle
                {
                    Title = "Test Article",
                    Description = "This is a test article.",
                    SourceUrl = "http://example.com",
                    PubDate = DateTime.UtcNow.ToString("o"),
                    ImageUrl = "http://example.com/image.jpg"
                }
            }
        };

        var httpClient = CreateMockHttpClient(newsResponse);
        var dbContext = CreateInMemoryDbContext();

        var fetcher = new NewsApiFetcher(_appConfigServiceMock.Object, config, _loggerMock.Object);

        // Act
        var article = await fetcher.FetchNewsDataApiNews(dbContext, httpClient);

        // Assert
        Assert.NotNull(article);
        Assert.Equal("Test Article", article.NewsArticles.Local.First().Title);
        Assert.Equal("NewsDataAPI", article.NewsArticles.Local.First().Source);
        Assert.Equal("This is a test article.", article.NewsArticles.Local.First().Content);
    }

        [Fact]
    public async Task FetchNewsDataApiNews_DescriptionEmpty_NoArticlesToDatabase()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "NewsDataApiKey", "test-key" }
        }).Build();

        var settings = new AppSettings
        {
            NewsAggregation = new NewsAggregationConfig
            {
                Filters = new FiltersConfig
                {
                    Language = "en",
                    Keywords = new List<string> { "test", "ai" }
                },
                MaxArticlesPerFetch = 5
            }
        };

        _appConfigServiceMock.Setup(a => a.GetConfigAsync()).ReturnsAsync(settings);

        var newsResponse = new NewsDataResponse
        {
            Results = new List<NewsDataArticle>
            {
                new NewsDataArticle
                {
                    Title = "Test Article",
                    SourceUrl = "http://example.com",
                    PubDate = DateTime.UtcNow.ToString("o"),
                    ImageUrl = "http://example.com/image.jpg"
                }
            }
        };

        var httpClient = CreateMockHttpClient(newsResponse);
        var dbContext = CreateInMemoryDbContext();

        var fetcher = new NewsApiFetcher(_appConfigServiceMock.Object, config, _loggerMock.Object);

        // Act
        var article = await fetcher.FetchNewsDataApiNews(dbContext, httpClient);

        // Assert
        Assert.Equal(0,article.NewsArticles.Local.Count);
    }
}
