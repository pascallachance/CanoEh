using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("api/[controller]")]
    public class LoginController(
        IConfiguration configuration,
        ILoginService loginService,
        IUserService userService,
        ISessionService sessionService) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILoginService _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        private readonly ISessionService _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginRequest))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Capture client information from headers
            var userAgent = Request.Headers.UserAgent.ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var loginResult = await _loginService.LoginAsync(request, userAgent, ipAddress);
            if (loginResult.IsFailure)
            {
                return StatusCode(loginResult.ErrorCode ?? 501, loginResult.Error);
            }
            var result = await _userService.UpdateLastLoginAsync(request.Email);
            if (result.IsFailure) {
                return StatusCode(result.ErrorCode ?? 501, result.Error);
            }

            var token = GenerateJwtToken(request.Email);
            var refreshToken = GenerateRefreshToken();
            
            // Store refresh token in database
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var refreshExpiryDays = Convert.ToDouble(jwtSettings["RefreshTokenExpiryDays"] ?? "30");
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshExpiryDays);
            
            var refreshTokenResult = await _userService.UpdateRefreshTokenAsync(request.Email, refreshToken, refreshExpiry);
            if (refreshTokenResult.IsFailure)
            {
                // Log but don't fail login - refresh token is optional
                System.Diagnostics.Debug.WriteLine($"Failed to store refresh token: {refreshTokenResult.Error}");
            }
            
            // Set access token cookie (short-lived)
            Response.Cookies.Append("AuthToken", token, new CookieOptions {
                HttpOnly = true,
                Secure = false, // true in production, false for local dev
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "15")),
                IsEssential = true
            });
            
            // Set refresh token cookie (long-lived)
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions {
                HttpOnly = true,
                Secure = false, // true in production, false for local dev
                SameSite = SameSiteMode.Lax,
                Expires = refreshExpiry,
                IsEssential = true
            });
            
            return Ok(new { 
                token, 
                sessionId = loginResult.Value?.SessionId,
                refreshToken = refreshToken // For debugging/testing only
            });
        }

        private string GenerateJwtToken(string email)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, email)
            };

            var key = new SymmetricSecurityKey(secretKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("Refresh token not found.");
            }

            var user = await _userService.FindByRefreshTokenAsync(refreshToken);
            if (user.IsFailure || user.Value == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            // Generate new access token
            var newAccessToken = GenerateJwtToken(user.Value.Email);
            
            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var refreshExpiryDays = Convert.ToDouble(jwtSettings["RefreshTokenExpiryDays"] ?? "30");
            var refreshExpiry = DateTime.UtcNow.AddDays(refreshExpiryDays);
            
            // Update refresh token in database
            var refreshTokenResult = await _userService.UpdateRefreshTokenAsync(user.Value.Email, newRefreshToken, refreshExpiry);
            if (refreshTokenResult.IsFailure)
            {
                return StatusCode(500, "Failed to update refresh token.");
            }

            // Set new access token cookie
            Response.Cookies.Append("AuthToken", newAccessToken, new CookieOptions {
                HttpOnly = true,
                Secure = false, // true in production, false for local dev
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "15")),
                IsEssential = true
            });
            
            // Set new refresh token cookie
            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions {
                HttpOnly = true,
                Secure = false, // true in production, false for local dev
                SameSite = SameSiteMode.Lax,
                Expires = refreshExpiry,
                IsEssential = true
            });

            return Ok(new { 
                token = newAccessToken,
                message = "Tokens refreshed successfully"
            });
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromQuery] Guid? sessionId = null, [FromHeader(Name = "X-Session-Id")] Guid? headerSessionId = null)
        {
            // Get email from JWT claims
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized("Invalid or missing authentication token.");
            }

            var result = await _userService.LogoutAsync(email);
            if (result.IsFailure)
            {
                return StatusCode(result.ErrorCode ?? 500, result.Error);
            }

            // Clear refresh token from database
            var clearRefreshResult = await _userService.ClearRefreshTokenAsync(email);
            if (clearRefreshResult.IsFailure)
            {
                // Log but don't fail logout
                Debug.WriteLine($"Failed to clear refresh token for {email}: {clearRefreshResult.Error}");
            }

            // Clear cookies
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");

            // Handle session logout if sessionId is provided
            var targetSessionId = sessionId ?? headerSessionId;
            if (targetSessionId.HasValue)
            {
                var sessionResult = await _sessionService.LogoutSessionAsync(targetSessionId.Value);
                if (sessionResult.IsFailure)
                {
                    // Log the error but don't fail the entire logout process
                    Debug.WriteLine($"Failed to logout session {targetSessionId}: {sessionResult.Error}");
                }
            }

            return Ok(new { message = "Logged out successfully.", email, sessionId = targetSessionId });
        }

        [Authorize]
        [HttpGet("user/claims")]
        public IActionResult GetUserClaims()
        {
            Debug.WriteLine($"Identity.Name: {User.Identity?.Name}");
            foreach (var claim in User.Claims)
                Debug.WriteLine($"{claim.Type}: {claim.Value}");

            return Ok();
        }

    }
}
