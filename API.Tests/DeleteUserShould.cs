using API.Controllers;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests
{
    public class DeleteUserShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public DeleteUserShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenUserDeletedSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var deleteResponse = new DeleteUserResponse
            {
                ID = Guid.NewGuid(),
                Firstname = "Test",
                Lastname = "User",
                Email = email,
                Phone = null,
                Lastlogin = null,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastupdatedAt = DateTime.UtcNow,
                Deleted = true
            };

            var result = Result.Success(deleteResponse);
            _mockUserService.Setup(s => s.DeleteUserAsync(email)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(deleteResponse, okResult.Value);
        }

        [Fact]
        public async Task ReturnForbidden_WhenUserTriesToDeleteAnotherUser()
        {
            // Arrange
            var targetEmail = "otheruser";
            var authenticatedEmail = "testuser";

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, authenticatedEmail) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(targetEmail);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.Equal("You can only delete your own user account.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailIsEmpty()
        {
            // Arrange
            var email = "";

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "testuser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(email);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Username is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistentuser";
            var result = Result.Failure<DeleteUserResponse>("User not found.", StatusCodes.Status404NotFound);
            _mockUserService.Setup(s => s.DeleteUserAsync(email)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(email);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal("User not found.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenUserIsAlreadyDeleted()
        {
            // Arrange
            var email = "testuser";
            var result = Result.Failure<DeleteUserResponse>("User is already deleted.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.DeleteUserAsync(email)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(email);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("User is already deleted.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var email = "testuser";
            _mockUserService.Setup(s => s.DeleteUserAsync(email)).ThrowsAsync(new Exception("Database error"));

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(email);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Contains("An error occurred", objectResult.Value?.ToString());
        }

        [Fact]
        public async Task ReturnBadRequest_WhenUsernameIsNull()
        {
            // Arrange
            string? email = null;

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "testuser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(email!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Username is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenUsernameIsWhitespace()
        {
            // Arrange
            var email = "   ";

            // Setup authenticated user context
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "testuser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.DeleteUser(email);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal("Username is required.", badRequestResult.Value);
        }
    }
}