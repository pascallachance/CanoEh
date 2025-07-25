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
            var username = "testuser";
            var getUserResponse = new GetUserResponse
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Phone = "1234567890",
                Lastlogin = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastupdatedAt = DateTime.UtcNow.AddDays(-2),
                Deleted = false
            };

            var result = Result.Success(getUserResponse);
            _mockUserService.Setup(s => s.GetUserAsync(username)).ReturnsAsync(result);

            // Set up the authenticated user context
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, username)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var returnedUser = Assert.IsType<GetUserResponse>(okResult.Value);
            Assert.Equal(getUserResponse.Uname, returnedUser.Uname);
            Assert.Equal(getUserResponse.Email, returnedUser.Email);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenUsernameIsEmpty()
        {
            // Act
            var response = await _controller.GetUser(string.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Username is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnForbid_WhenUserTriesToAccessAnotherUser()
        {
            // Arrange
            var authenticatedUsername = "user1";
            var requestedUsername = "user2";

            // Set up the authenticated user context
            var claims = new List<Claim>
           {
               new(ClaimTypes.NameIdentifier, authenticatedUsername)
           };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(requestedUsername);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.Equal("You can only access your own user information.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var username = "nonexistentuser";
            var result = Result.Failure<GetUserResponse>("User not found.", StatusCodes.Status404NotFound);
            _mockUserService.Setup(s => s.GetUserAsync(username)).ReturnsAsync(result);

            // Set up the authenticated user context
            var claims = new List<Claim>
           {
               new(ClaimTypes.NameIdentifier, username)
           };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(username);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal("User not found.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnGetUserResponse_WithoutPasswordField()
        {
            // Arrange
            var username = "testuser";
            var getUserResponse = new GetUserResponse
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Phone = "1234567890",
                Lastlogin = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastupdatedAt = DateTime.UtcNow.AddDays(-2),
                Deleted = false
            };

            var result = Result.Success(getUserResponse);
            _mockUserService.Setup(s => s.GetUserAsync(username)).ReturnsAsync(result);

            // Set up the authenticated user context
            var claims = new List<Claim>
           {
               new(ClaimTypes.NameIdentifier, username)
           };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetUser(username);

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