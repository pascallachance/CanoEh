using API.Controllers;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace API.Tests
{
    public class LogoutShould
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILoginService> _mockLoginService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISessionService> _mockSessionService;
        private readonly LoginController _controller;

        public LogoutShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLoginService = new Mock<ILoginService>();
            _mockUserService = new Mock<IUserService>();
            _mockSessionService = new Mock<ISessionService>();
            _controller = new LoginController(_mockConfiguration.Object, _mockLoginService.Object, _mockUserService.Object, _mockSessionService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenUserLoggedOutSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var result = Result.Success(true);
            _mockUserService.Setup(s => s.LogoutAsync(email)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            // Verify the response content
            var responseValue = okResult.Value;
            Assert.NotNull(responseValue);
            
            // Use reflection to check anonymous object properties
            var messageProperty = responseValue.GetType().GetProperty("message");
            var emailProperty = responseValue.GetType().GetProperty("email");
            
            Assert.NotNull(messageProperty);
            Assert.NotNull(emailProperty);
            Assert.Equal("Logged out successfully.", messageProperty.GetValue(responseValue));
            Assert.Equal(email, emailProperty.GetValue(responseValue));

            _mockUserService.Verify(s => s.LogoutAsync(email), Times.Once);
        }

        [Fact]
        public async Task ReturnUnauthorized_WhenNoAuthenticationToken()
        {
            // Arrange - No authenticated user context (no claims)
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var response = await _controller.Logout();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
            Assert.Equal("Invalid or missing authentication token.", unauthorizedResult.Value);

            _mockUserService.Verify(s => s.LogoutAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ReturnUnauthorized_WhenUsernameClaimMissing()
        {
            // Arrange - Authenticated but without NameIdentifier claim
            var claims = new List<Claim> { new(ClaimTypes.Name, "testuser") }; // Wrong claim type
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.Logout();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
            Assert.Equal("Invalid or missing authentication token.", unauthorizedResult.Value);

            _mockUserService.Verify(s => s.LogoutAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var result = Result.Failure<bool>("User not found.", StatusCodes.Status404NotFound);
            _mockUserService.Setup(s => s.LogoutAsync(email)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.Logout();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal("User not found.", objectResult.Value);

            _mockUserService.Verify(s => s.LogoutAsync(email), Times.Once);
        }

        [Fact]
        public async Task ReturnGone_WhenUserIsDeleted()
        {
            // Arrange
            var email = "deleted@example.com";
            var result = Result.Failure<bool>("User is deleted.", StatusCodes.Status410Gone);
            _mockUserService.Setup(s => s.LogoutAsync(email)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.Logout();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status410Gone, objectResult.StatusCode);
            Assert.Equal("User is deleted.", objectResult.Value);

            _mockUserService.Verify(s => s.LogoutAsync(email), Times.Once);
        }

        [Fact]
        public async Task ReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var email = "test@example.com";
            var result = Result.Failure<bool>("Database connection failed.", StatusCodes.Status500InternalServerError);
            _mockUserService.Setup(s => s.LogoutAsync(email)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.Logout();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("Database connection failed.", objectResult.Value);

            _mockUserService.Verify(s => s.LogoutAsync(email), Times.Once);
        }
    }
}