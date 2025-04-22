namespace SocialAggregatorAPI.Tests;

using Microsoft.Extensions.Configuration;
using SocialAggregatorAPI.Helpers;
using Xunit;

public class JwtReaderTests
{
    private readonly JwtReader _jwtReader;

    public JwtReaderTests()
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

        _jwtReader = new JwtReader(config);
    }

    [Fact]
    public void GetKey_ReturnsCorrectKey()
    {
        var key = _jwtReader.GetKey();
        Assert.Equal("YourSecretKeyThatIsAtLeast32CharactersLong", key);
    }
}
