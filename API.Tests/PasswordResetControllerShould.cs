using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.Tests
{
    public class PasswordResetControllerShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly PasswordResetController _controller;

        public PasswordResetControllerShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new PasswordResetController(_mockUserService.Object);
        }

        [Fact]
        public async Task ForgotPassword_ReturnOk_WhenValidRequest()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "test@example.com" };
            var response = new ForgotPasswordResponse 
            { 
                Email = "test@example.com",
                Message = "If the email address exists in our system, you will receive a password reset link shortly."
            };

            _mockUserService.Setup(s => s.ForgotPasswordAsync(request))
                .ReturnsAsync(Result.Success(response));

            // Act
            var result = await _controller.ForgotPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsType<ForgotPasswordResponse>(okResult.Value);
            Assert.Equal("test@example.com", returnedResponse.Email);
            Assert.Contains("If the email address exists", returnedResponse.Message);
        }

        [Fact]
        public async Task ForgotPassword_ReturnBadRequest_WhenInvalidEmail()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "invalid-email" };

            _mockUserService.Setup(s => s.ForgotPasswordAsync(request))
                .ReturnsAsync(Result.Failure<ForgotPasswordResponse>("Invalid email format.", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.ForgotPassword(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
            Assert.Equal("Invalid email format.", statusResult.Value);
        }

        [Fact]
        public async Task ResetPassword_ReturnOk_WhenValidRequest()
        {
            // Arrange
            var request = new ResetPasswordRequest 
            { 
                Token = "validtoken",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };
            var response = new ResetPasswordResponse 
            { 
                Message = "Password has been reset successfully.",
                ResetAt = DateTime.UtcNow
            };

            _mockUserService.Setup(s => s.ResetPasswordAsync(request))
                .ReturnsAsync(Result.Success(response));

            // Act
            var result = await _controller.ResetPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResponse = Assert.IsType<ResetPasswordResponse>(okResult.Value);
            Assert.Equal("Password has been reset successfully.", returnedResponse.Message);
        }

        [Fact]
        public async Task ResetPassword_ReturnBadRequest_WhenInvalidToken()
        {
            // Arrange
            var request = new ResetPasswordRequest 
            { 
                Token = "invalidtoken",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };

            _mockUserService.Setup(s => s.ResetPasswordAsync(request))
                .ReturnsAsync(Result.Failure<ResetPasswordResponse>("Invalid or expired reset token.", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.ResetPassword(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
            Assert.Equal("Invalid or expired reset token.", statusResult.Value);
        }

        [Fact]
        public async Task ResetPassword_ReturnBadRequest_WhenPasswordsDontMatch()
        {
            // Arrange
            var request = new ResetPasswordRequest 
            { 
                Token = "validtoken",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "differentpassword123"
            };

            _mockUserService.Setup(s => s.ResetPasswordAsync(request))
                .ReturnsAsync(Result.Failure<ResetPasswordResponse>("New password and confirm new password do not match.", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.ResetPassword(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
            Assert.Equal("New password and confirm new password do not match.", statusResult.Value);
        }
    }
}