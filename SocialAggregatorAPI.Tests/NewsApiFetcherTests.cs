namespace SocialAggregatorAPI.Tests;

using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SocialAggregatorAPI.Data;
using SocialAggregatorAPI.Models;

public class NewsApiFetcherTests
{
    [Fact]
    public async Task FetchNewsDataApiNews_ShouldAddArticles_WhenValidResponseReceived()
    {
        // Arrange
        var mockConfigReader = new Mock<INewsAggregationConfigReader>();
        mockConfigReader.Setup(c => c.GetSettings()).Returns(new NewsAggregationSettings
        {
            MaxArticlesPerFetch = 5,
            Filters = new FilterSettings
            {
                Language = "en",
                Keywords = new List<string> { "AI", "Blockchain" }
            }
        });

        var inMemorySettings = new Dictionary<string, string> {
            { "NewsDataApiKey", "dummy_api_key" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var mockLogger = new Mock<ILogger<NewsApiFetcher>>();

        // Fake JSON response
        var fakeApiResponse = new NewsDataResponse
        {
            Results = new List<NewsDataArticle>
            {
                new NewsDataArticle
                {
                    Title = "AI & Blockchain Revolution",
                    Description = "A brief look into how AI and blockchain are changing tech.",
                    SourceUrl = "https://example.com/article",
                    PubDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    ImageUrl = "https://example.com/image.jpg"
                }
            }
        };

        var json = JsonSerializer.Serialize(fakeApiResponse);

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(handler.Object);

        // Use a shared in-memory database name
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("SharedTestDb")  // This ensures both contexts use the same DB
            .Options;

        var dbContext = new AppDbContext(options);
        var fetcher = new NewsApiFetcher(mockConfigReader.Object, configuration, mockLogger.Object);

        // Act
        var updatedDbContext = await fetcher.FetchNewsDataApiNews(dbContext, httpClient);
        await dbContext.SaveChangesAsync();


        // Assert
        Assert.NotNull(updatedDbContext);  // Ensure the context is not null
        var articles = await updatedDbContext.NewsArticles.ToListAsync();  // Ensure we get all articles added
        Assert.Single(articles);  // Assert that exactly one article is present

        var article = articles.First();
        Assert.Equal("AI & Blockchain Revolution", article.Title);  // Validate title
        Assert.Equal("A brief look into how AI and blockchain are changing tech.", article.Content);  // Validate content
        Assert.Equal("NewsDataAPI", article.Source);  // Validate source
        Assert.Equal("https://example.com/article", article.Url);  // Validate URL
    }
}
