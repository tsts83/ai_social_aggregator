namespace SocialAggregatorAPI.Tests;

using Microsoft.Extensions.Options;
using Moq;
using SocialAggregatorAPI.Models;
using Xunit;

public class NewsAggregationConfigServiceTests
    {
        [Fact]
        public void GetSettings_ReturnsCorrectConfiguration()
        {
            // Arrange
            var mockSettings = new NewsAggregationSettings
            {
                FetchIntervalMinutes = 60,
                MaxArticlesPerFetch = 10,
                Filters = new FilterSettings
                {
                    Keywords = new List<string> { "Technology", "Science" },
                    Language = "en"
                },
                SourcePreferences = new SourcePreferences
                {
                    Reddit = false,
                    NewsDataAPI = true,
                    HackerNews = false,
                    Nitter = false
                },
                PostSettings = new PostSettings
                {
                    PostIntervalMinutes = 30,
                    MaxPostsPerDay = 5
                }
            };

            var mockOptions = new Mock<IOptions<NewsAggregationSettings>>();
            mockOptions.Setup(opt => opt.Value).Returns(mockSettings);

            var service = new NewsAggregationConfigReader(mockOptions.Object);

            // Act
            var result = service.GetSettings();

            // Assert
            Assert.Equal(mockSettings.FetchIntervalMinutes, result.FetchIntervalMinutes);
            Assert.Equal(mockSettings.Filters.Language, result.Filters.Language);
            Assert.False(result.SourcePreferences.Reddit);
            Assert.False(result.SourcePreferences.HackerNews);
            Assert.Contains("Technology", result.Filters.Keywords);
            Assert.Equal(5, result.PostSettings.MaxPostsPerDay);
        }
    }

