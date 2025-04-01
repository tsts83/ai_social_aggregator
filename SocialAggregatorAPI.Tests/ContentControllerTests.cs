namespace SocialAggregatorAPI.Tests;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

public class ContentControllerTests
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public ContentControllerTests()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        // [Fact]
        // public async Task GetTrendingPosts_ReturnsPosts_WhenRedditIntegrationIsCorrect()
        // {
        //     // Mocking Reddit API call if needed, or assume the controller method is working correctly
        //     var response = await _client.GetAsync("/api/content/trending");

        //     // Assert
        //     response.EnsureSuccessStatusCode(); // Ensure the response is 200 OK
        //     var responseBody = await response.Content.ReadAsStringAsync();
        //     Assert.Contains("Reddit", responseBody); // Check if Reddit data is in the response
        // }

        // [Fact]
        // public async Task GetTopNews_ReturnsArticles_WhenNewsAPIIntegrationWorks()
        // {
        //     var response = await _client.GetAsync("/api/content/news");

        //     // Assert
        //     response.EnsureSuccessStatusCode(); // Ensure the response is 200 OK
        //     var responseBody = await response.Content.ReadAsStringAsync();
        //     Assert.Contains("headline", responseBody); // Check if news articles are present
        // }
    }


