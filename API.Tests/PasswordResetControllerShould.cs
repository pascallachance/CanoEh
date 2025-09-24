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

    public class ForgotPasswordResponseLocaleShould
    {
        [Fact]
        public void GetLocalizedMessage_ReturnEnglishMessage_WhenEnglishLocale()
        {
            // Act
            var message = ForgotPasswordResponse.DefaultMessages.GetLocalizedMessage("en");

            // Assert
            Assert.Equal("If the email address exists in our system, you will receive a password reset link shortly.", message);
        }

        [Fact]
        public void GetLocalizedMessage_ReturnFrenchMessage_WhenFrenchLocale()
        {
            // Act
            var message = ForgotPasswordResponse.DefaultMessages.GetLocalizedMessage("fr");

            // Assert
            Assert.Equal("Si l'adresse e-mail existe dans notre système, vous recevrez bientôt un lien de réinitialisation du mot de passe.", message);
        }

        [Fact]
        public void GetLocalizedMessage_ReturnEnglishMessage_WhenUnknownLocale()
        {
            // Act
            var message = ForgotPasswordResponse.DefaultMessages.GetLocalizedMessage("de");

            // Assert
            Assert.Equal("If the email address exists in our system, you will receive a password reset link shortly.", message);
        }

        [Fact]
        public void GetLocalizedMessage_ReturnEnglishMessage_WhenNullLocale()
        {
            // Act
            var message = ForgotPasswordResponse.DefaultMessages.GetLocalizedMessage(null);

            // Assert
            Assert.Equal("If the email address exists in our system, you will receive a password reset link shortly.", message);
        }

        [Fact]
        public void CreateWithLocale_SetsCorrectLocalizedMessage()
        {
            // Act
            var responseEn = ForgotPasswordResponse.CreateWithLocale("test@example.com", "en");
            var responseFr = ForgotPasswordResponse.CreateWithLocale("test@example.com", "fr");

            // Assert
            Assert.Equal("test@example.com", responseEn.Email);
            Assert.Equal("If the email address exists in our system, you will receive a password reset link shortly.", responseEn.Message);
            
            Assert.Equal("test@example.com", responseFr.Email);
            Assert.Equal("Si l'adresse e-mail existe dans notre système, vous recevrez bientôt un lien de réinitialisation du mot de passe.", responseFr.Message);
        }

        [Fact]
        public void CreateWithLocale_WithCustomMessage_OverridesLocale()
        {
            // Act
            var response = ForgotPasswordResponse.CreateWithLocale("test@example.com", "fr", "Custom message");

            // Assert
            Assert.Equal("test@example.com", response.Email);
            Assert.Equal("Custom message", response.Message);
        }
    }
}