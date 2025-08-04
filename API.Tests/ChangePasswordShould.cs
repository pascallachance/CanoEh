using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests
{
    public class ChangePasswordShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public ChangePasswordShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenPasswordChangedSuccessfully()
        {
            // Arrange
            var username = "testuser";
            var changePasswordRequest = new ChangePasswordRequest
            {
                Email = username,
                CurrentPassword = "oldpassword123",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "newpassword456"
            };

            var changePasswordResponse = new ChangePasswordResponse
            {
                Email = username,
                LastUpdatedAt = DateTime.UtcNow,
                Message = "Password changed successfully."
            };

            var result = Result.Success(changePasswordResponse);
            _mockUserService.Setup(s => s.ChangePasswordAsync(changePasswordRequest)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.ChangePassword(changePasswordRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(changePasswordResponse, okResult.Value);
        }

        [Fact]
        public async Task ReturnForbidden_WhenUserTriesToChangeAnotherUsersPassword()
        {
            // Arrange
            var authenticatedUser = "user1";
            var targetUser = "user2";
            var changePasswordRequest = new ChangePasswordRequest
            {
                Email = targetUser,
                CurrentPassword = "oldpassword123",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "newpassword456"
            };

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, authenticatedUser) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.ChangePassword(changePasswordRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
            Assert.Equal("You can only change your own password.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var username = "testuser";
            var changePasswordRequest = new ChangePasswordRequest
            {
                Email = username,
                CurrentPassword = "wrongpassword",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "newpassword456"
            };

            var result = Result.Failure<ChangePasswordResponse>("Current password is incorrect.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.ChangePasswordAsync(changePasswordRequest)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.ChangePassword(changePasswordRequest);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Current password is incorrect.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var username = "testuser";
            var changePasswordRequest = new ChangePasswordRequest
            {
                Email = username,
                CurrentPassword = "oldpassword123",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "newpassword456"
            };

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Make ModelState invalid
            _controller.ModelState.AddModelError("Username", "Username is required");

            // Act
            var response = await _controller.ChangePassword(changePasswordRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }
    }
}