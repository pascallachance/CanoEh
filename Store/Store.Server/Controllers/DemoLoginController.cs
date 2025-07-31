using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Store.Server.Controllers
{
    [Route("api/store/[controller]")]
    public class DemoLoginController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Demo users for testing (in production, this would come from database)
        private readonly Dictionary<string, string> _demoUsers = new()
        {
            { "testuser123", "password123" },
            { "demouser1", "password123" },
            { "storeuser", "password123" }
        };

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] DemoLoginRequest request)
        {
            await Task.Delay(500); // Simulate database call

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Validate minimum length requirements
            if (request.Username.Length < 8 || request.Password.Length < 8)
            {
                return BadRequest("Username and password must be at least 8 characters long.");
            }

            // Check demo credentials
            if (!_demoUsers.TryGetValue(request.Username, out var expectedPassword) || 
                expectedPassword != request.Password)
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(request.Username);
            
            // Set HTTP-only cookie with security flags
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,      // Prevent XSS attacks
                Secure = false,       // Allow HTTP for development (should be true in production)
                SameSite = SameSiteMode.Lax, // Less strict for development
                Expires = DateTime.UtcNow.AddMinutes(30),
                IsEssential = true    // Required for authentication
            };
            
            Response.Cookies.Append("AuthToken", token, cookieOptions);
            
            // Generate CSRF token for additional protection
            var csrfToken = Guid.NewGuid().ToString();
            var csrfCookieOptions = new CookieOptions
            {
                HttpOnly = false,     // Accessible to JavaScript for CSRF protection
                Secure = false,       // Allow HTTP for development
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };
            Response.Cookies.Append("X-CSRF-Token", csrfToken, csrfCookieOptions);
            
            return Ok(new { 
                message = "Login successful",
                sessionId = Guid.NewGuid(),
                csrfToken = csrfToken
            });
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "YourSecretKeyHereForDevelopment123456789012345678901234567890");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.NameIdentifier, username)
            };

            var key = new SymmetricSecurityKey(secretKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "StoreApp",
                audience: jwtSettings["Audience"] ?? "StoreClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // Clear authentication cookies (no authorization required for demo)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,       // Allow HTTP for development
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(-1) // Expire immediately
            };
            
            Response.Cookies.Append("AuthToken", "", cookieOptions);
            Response.Cookies.Append("X-CSRF-Token", "", cookieOptions);

            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAuthStatus()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var username = isAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
            
            return Ok(new { 
                isAuthenticated,
                username
            });
        }

        [HttpGet("demo-credentials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetDemoCredentials()
        {
            return Ok(new { 
                message = "Demo credentials for testing",
                users = _demoUsers.Keys.ToArray(),
                note = "All demo users use 'password123' as password"
            });
        }
    }

    public class DemoLoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}