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
    public class PasswordResetIntegrationShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly PasswordResetController _controller;

        public PasswordResetIntegrationShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new PasswordResetController(_mockUserService.Object);
            
            // Setup HttpContext for the controller
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task CompletePasswordResetFlow_WhenValidInputs()
        {
            // Arrange - Step 1: Request password reset
            var forgotPasswordRequest = new ForgotPasswordRequest { Email = "user@example.com" };
            var forgotPasswordResponse = new ForgotPasswordResponse 
            { 
                Email = "user@example.com",
                Message = "If the email address exists in our system, you will receive a password reset link shortly."
            };

            _mockUserService.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>()))
                .ReturnsAsync(Result.Success(forgotPasswordResponse));

            // Act - Step 1: Call forgot password
            var forgotResult = await _controller.ForgotPassword(forgotPasswordRequest);

            // Assert - Step 1: Verify forgot password worked
            var forgotOkResult = Assert.IsType<OkObjectResult>(forgotResult);
            var forgotResponse = Assert.IsType<ForgotPasswordResponse>(forgotOkResult.Value);
            Assert.Equal("user@example.com", forgotResponse.Email);
            Assert.Contains("If the email address exists", forgotResponse.Message);

            // Arrange - Step 2: Reset password with token
            var resetPasswordRequest = new ResetPasswordRequest 
            { 
                Token = "valid-reset-token",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };
            var resetPasswordResponse = new ResetPasswordResponse 
            { 
                Message = "Password has been reset successfully.",
                ResetAt = DateTime.UtcNow
            };

            _mockUserService.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>()))
                .ReturnsAsync(Result.Success(resetPasswordResponse));

            // Act - Step 2: Call reset password
            var resetResult = await _controller.ResetPassword(resetPasswordRequest);

            // Assert - Step 2: Verify password reset worked
            var resetOkResult = Assert.IsType<OkObjectResult>(resetResult);
            var resetResponse = Assert.IsType<ResetPasswordResponse>(resetOkResult.Value);
            Assert.Equal("Password has been reset successfully.", resetResponse.Message);
            Assert.True(resetResponse.ResetAt > DateTime.UtcNow.AddMinutes(-1));

            // Verify service methods were called
            _mockUserService.Verify(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>()), Times.Once);
            _mockUserService.Verify(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_AlwaysReturnSuccess_ForSecurityReasons()
        {
            // Arrange - Test with non-existent email
            var request1 = new ForgotPasswordRequest { Email = "nonexistent@example.com" };
            var request2 = new ForgotPasswordRequest { Email = "existing@example.com" };
            
            var response = new ForgotPasswordResponse 
            { 
                Email = "test@example.com",
                Message = "If the email address exists in our system, you will receive a password reset link shortly."
            };

            _mockUserService.Setup(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>()))
                .ReturnsAsync(Result.Success(response));

            // Act
            var result1 = await _controller.ForgotPassword(request1);
            var result2 = await _controller.ForgotPassword(request2);

            // Assert - Both should return success to prevent email enumeration
            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
            
            _mockUserService.Verify(s => s.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>()), Times.Exactly(2));
        }

        [Fact]
        public async Task PasswordReset_RejectInvalidTokens()
        {
            // Arrange
            var request = new ResetPasswordRequest 
            { 
                Token = "invalid-token",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };

            _mockUserService.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>()))
                .ReturnsAsync(Result.Failure<ResetPasswordResponse>("Invalid or expired reset token.", StatusCodes.Status400BadRequest));

            // Act
            var result = await _controller.ResetPassword(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
            Assert.Equal("Invalid or expired reset token.", statusResult.Value);
        }
    }
}