using Domain.Models.Requests;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class ChangePasswordValidationShould
    {
        [Fact]
        public void ReturnFailure_WhenUsernameIsEmpty()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = "",
                CurrentPassword = "oldpassword123",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "newpassword456"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Username is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenCurrentPasswordIsEmpty()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = "testuser123",
                CurrentPassword = "",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "newpassword456"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Current password is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenNewPasswordIsTooShort()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = "testuser123",
                CurrentPassword = "oldpassword123",
                NewPassword = "short",
                ConfirmNewPassword = "short"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("New password must be at least 8 characters long.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = "testuser123",
                CurrentPassword = "oldpassword123",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "differentpassword"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("New password and confirm new password do not match.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenNewPasswordSameAsCurrentPassword()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = "testuser123",
                CurrentPassword = "samepassword123",
                NewPassword = "samepassword123",
                ConfirmNewPassword = "samepassword123"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("New password must be different from current password.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnSuccess_WhenAllValidationsPassed()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                Username = "testuser123",
                CurrentPassword = "oldpassword123",
                NewPassword = "newpassword456",
                ConfirmNewPassword = "newpassword456"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}