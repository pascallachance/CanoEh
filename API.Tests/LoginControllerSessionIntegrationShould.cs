using API.Controllers;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace API.Tests
{
    public class LoginControllerSessionIntegrationShould
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILoginService> _mockLoginService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISessionService> _mockSessionService;
        private readonly LoginController _controller;

        public LoginControllerSessionIntegrationShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLoginService = new Mock<ILoginService>();
            _mockUserService = new Mock<IUserService>();
            _mockSessionService = new Mock<ISessionService>();

            // Setup JWT configuration mocks
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["Secret"]).Returns("ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345");
            jwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
            jwtSection.Setup(x => x["ExpiryMinutes"]).Returns("60");
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);

            _controller = new LoginController(_mockConfiguration.Object, _mockLoginService.Object, _mockUserService.Object, _mockSessionService.Object);
            
            // Setup HttpContext for the controller
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Login_ReturnSessionIdInResponse()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var loginRequest = new LoginRequest
            {
                Username = "testuser12345",
                Password = "testpass123"
            };

            var loginResponse = new Domain.Models.Responses.LoginResponse
            {
                SessionId = sessionId
            };

            _mockLoginService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                           .ReturnsAsync(Result.Success(loginResponse));
            _mockUserService.Setup(s => s.UpdateLastLoginAsync(It.IsAny<string>()))
                          .ReturnsAsync(Result.Success(true));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseValue = okResult.Value;
            
            // Use reflection to check the anonymous object properties
            var tokenProperty = responseValue?.GetType().GetProperty("token");
            var sessionIdProperty = responseValue?.GetType().GetProperty("sessionId");
            
            Assert.NotNull(tokenProperty);
            Assert.NotNull(sessionIdProperty);
            Assert.Equal(sessionId, sessionIdProperty.GetValue(responseValue));
        }

        [Fact]
        public async Task Logout_CallSessionServiceWhenSessionIdProvided()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            
            _mockUserService.Setup(s => s.LogoutAsync(It.IsAny<string>()))
                          .ReturnsAsync(Result.Success(true));
            _mockSessionService.Setup(s => s.LogoutSessionAsync(sessionId))
                             .ReturnsAsync(Result.Success(new Session { SessionId = sessionId }));

            // Setup authenticated user context
            var claims = new System.Security.Claims.Claim[]
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, "testuser12345")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;

            // Act
            var result = await _controller.Logout(sessionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockSessionService.Verify(s => s.LogoutSessionAsync(sessionId), Times.Once);
            
            var responseValue = okResult.Value;
            var sessionIdProperty = responseValue?.GetType().GetProperty("sessionId");
            Assert.Equal(sessionId, sessionIdProperty?.GetValue(responseValue));
        }

        [Fact]
        public async Task Logout_NotCallSessionServiceWhenNoSessionIdProvided()
        {
            // Arrange
            _mockUserService.Setup(s => s.LogoutAsync(It.IsAny<string>()))
                          .ReturnsAsync(Result.Success(true));

            // Setup authenticated user context
            var claims = new System.Security.Claims.Claim[]
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, "testuser12345")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            _controller.ControllerContext.HttpContext.User = principal;

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockSessionService.Verify(s => s.LogoutSessionAsync(It.IsAny<Guid>()), Times.Never);
            
            var responseValue = okResult.Value;
            var sessionIdProperty = responseValue?.GetType().GetProperty("sessionId");
            Assert.Null(sessionIdProperty?.GetValue(responseValue));
        }
    }
}