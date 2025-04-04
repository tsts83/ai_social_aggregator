using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SocialAggregatorAPI.Controllers;
using SocialAggregatorAPI.Data;

namespace SocialAggregatorAPI.Tests
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly AppDbContext _dbContext;

        public AuthControllerTests()
        {

                    // Mock configuration
            var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "YourSecretKeyThatIsAtLeast32CharactersLong" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            })
            .Build();

            // Use a separate in-memory database for each test run
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString()) 
                .Options;

            _dbContext = new AppDbContext(options);
            _controller = new AuthController(_dbContext, config);
        }

        [Fact]
        public async Task Register_ReturnsSuccess_WhenUserIsValid()
        {
            // Arrange
            var testUser = new User
            {
                Username = "newuser",
                Password = "ValidPassword123",
                Email = "newuser@example.com"
            };

            // Act
            var result = await _controller.Register(testUser) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("User registered successfully", result.Value);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameIsTaken()
        {
            // Arrange
            var testUser = new User
            {
                Username = "existinguser",
                Password = "ValidPassword123",
                Email = "existinguser@example.com"
            };

            await _controller.Register(testUser); // Register first

            var duplicateUser = new User
            {
                Username = "existinguser",
                Password = "AnotherPassword",
                Email = "newemail@example.com"
            };

            // Act
            var result = await _controller.Register(duplicateUser) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Username already taken", result.Value);
        }

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            var testUser = new User
            {
                Username = "testuser",
                Password = "TestPassword123",
                Email = "testuser@example.com"
            };

            // Simulate registration
            await _controller.Register(testUser);

            var loginUser = new User
            {
                Username = "testuser",
                Password = "TestPassword123"
            };

            // Act
            var result = await _controller.Login(loginUser) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("token", result.Value.ToString());
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreIncorrect()
        {
            // Arrange
            var loginUser = new User
            {
                Username = "wronguser",
                Password = "WrongPassword123"
            };

            // Act
            var result = await _controller.Login(loginUser) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Invalid credentials", result.Value);
        }
    }
}
