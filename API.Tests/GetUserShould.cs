using System.Security.Claims;
using API.Controllers;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests
{
    public class GetUserShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public GetUserShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenUserFoundSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var getUserResponse = new GetUserResponse
            {
                ID = Guid.NewGuid(),
                Email = email,
                Firstname = "Test",
                Lastname = "User",
                Phone = "1234567890",
                Lastlogin = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastupdatedAt = DateTime.UtcNow.AddDays(-2),
                Deleted = false,
                ValidEmail = true
            };

            var result = Result.Success(getUserResponse);
            _mockUserService.Setup(s => s.GetUserAsync(email)).ReturnsAsync(result);

            // Set up the authenticated user context
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, email)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var returnedUser = Assert.IsType<GetUserResponse>(okResult.Value);
            Assert.Equal(getUserResponse.Email, returnedUser.Email);
            Assert.Equal(getUserResponse.Email, returnedUser.Email);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailIsEmpty()
        {
            // Act
            var response = await _controller.GetUser(string.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Email is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnForbid_WhenUserTriesToAccessAnotherUser()
        {
            // Arrange
            var authenticatedEmail = "user1@example.com";
            var requestedEmail = "user2@example.com";

            // Set up the authenticated user context
            var claims = new List<Claim>
           {
               new(ClaimTypes.NameIdentifier, authenticatedEmail)
           };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(requestedEmail);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.Equal("You can only access your own user information.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var result = Result.Failure<GetUserResponse>("User not found.", StatusCodes.Status404NotFound);
            _mockUserService.Setup(s => s.GetUserAsync(email)).ReturnsAsync(result);

            // Set up the authenticated user context
            var claims = new List<Claim>
           {
               new(ClaimTypes.NameIdentifier, email)
           };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(email);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal("User not found.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnGetUserResponse_WithoutPasswordField()
        {
            // Arrange
            var email = "test@example.com";
            var getUserResponse = new GetUserResponse
            {
                ID = Guid.NewGuid(),
                Email = email,
                Firstname = "Test",
                Lastname = "User",
                Phone = "1234567890",
                Lastlogin = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastupdatedAt = DateTime.UtcNow.AddDays(-2),
                Deleted = false,
                ValidEmail = true
            };

            var result = Result.Success(getUserResponse);
            _mockUserService.Setup(s => s.GetUserAsync(email)).ReturnsAsync(result);

            // Set up the authenticated user context
            var claims = new List<Claim>
           {
               new(ClaimTypes.NameIdentifier, email)
           };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnedUser = Assert.IsType<GetUserResponse>(okResult.Value);

            // Verify that the response is GetUserResponse type and not User entity
            Assert.IsType<GetUserResponse>(returnedUser);

            // Verify no password field is exposed (GetUserResponse doesn't have Password property)
            var responseType = returnedUser.GetType();
            Assert.Null(responseType.GetProperty("Password"));
        }
    }
}