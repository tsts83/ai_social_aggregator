using System.Text;

namespace SocialAggregatorAPI.Helpers
{
    public class JwtReader
    {
        // Properties to hold the values
        public string? Key { get; private set; }
        public string? Issuer { get; private set; }
        public string? Audience { get; private set; }

        // Constructor: Reads from the config file
        public JwtReader(IConfiguration configuration)
        {
            // Access the JWT settings from the configuration
            var jwtKey = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");

            // Check if the settings exist; if not, throw an exception
            if (jwtKey == null )
            {
                throw new Exception("Jwt settings are not properly configured.");
            }

            // Set the properties based on the configuration values
            Key = jwtKey;
        }

        // Getter methods to access the values
        public string GetKey()
        {
            var key = Key;
            
            if (string.IsNullOrEmpty(key) || Encoding.UTF8.GetBytes(key).Length < 32)
            {
                throw new Exception("JWT Key is too short. It must be at least 32 bytes.");
            }

            return key;
        }
    }
}
