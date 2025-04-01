namespace SocialAggregatorAPI.Tests;

using Xunit;
using BCrypt.Net;

public class PasswordHashingTests
{
    [Fact]
    public void HashPassword_CreatesValidHash()
    {
        // Arrange
        var password = "SecurePassword123";

        // Act
        var hashedPassword = BCrypt.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.True(hashedPassword.Length > 0);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "SecurePassword123";
        var hashedPassword = BCrypt.HashPassword(password);

        // Act
        var isMatch = BCrypt.Verify(password, hashedPassword);

        // Assert
        Assert.True(isMatch);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword123";
        var wrongPassword = "WrongPassword";
        var hashedPassword = BCrypt.HashPassword(password);

        // Act
        var isMatch = BCrypt.Verify(wrongPassword, hashedPassword);

        // Assert
        Assert.False(isMatch);
    }
}
