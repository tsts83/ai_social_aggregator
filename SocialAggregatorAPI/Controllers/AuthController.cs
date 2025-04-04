using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SocialAggregatorAPI.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SocialAggregatorAPI.Helpers;

namespace SocialAggregatorAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest("Username already taken");

            if (string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Password is required");

            // Hash the password before saving to the database
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Remove the plain-text password (password field) before saving to DB
            user.Password = "";

            // Save the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginUser.Username);
            if (user == null || string.IsNullOrEmpty(user.Username) || !BCrypt.Net.BCrypt.Verify(loginUser.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }
    private string GenerateJwtToken(User user)
    {
        var jwtReader = new JwtReader(_config);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, 
                new DateTimeOffset(DateTime.UtcNow.AddHours(2)).ToUnixTimeSeconds().ToString()) // Fix expiration
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtReader.GetKey()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtReader.GetIssuer(),
            audience: jwtReader.GetAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), 
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedRoute()
        {
            return Ok("You have access!");
        }
    }
}
