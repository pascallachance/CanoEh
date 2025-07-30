using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Implementations;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace API.Tests
{
    public class ResetPasswordShould
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserService _userService;

        public ResetPasswordShould()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task ReturnFailure_WhenTokenIsEmpty()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Token = "",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };

            // Act
            var result = await _userService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Reset token is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenNewPasswordIsEmpty()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Token = "validtoken",
                NewPassword = "",
                ConfirmNewPassword = ""
            };

            // Act
            var result = await _userService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("New password is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Token = "validtoken",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "differentpassword123"
            };

            // Act
            var result = await _userService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("New password and confirm new password do not match.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenPasswordIsTooShort()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Token = "validtoken",
                NewPassword = "short",
                ConfirmNewPassword = "short"
            };

            // Act
            var result = await _userService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("New password must be at least 8 characters long.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenTokenIsInvalidOrExpired()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Token = "invalidtoken",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };

            _mockUserRepository.Setup(r => r.FindByPasswordResetTokenAsync("invalidtoken"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Invalid or expired reset token.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenUserIsDeleted()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Token = "validtoken",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };

            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "oldhashedpassword",
                Deleted = true,
                ValidEmail = true,
                Createdat = DateTime.UtcNow,
                PasswordResetToken = "validtoken",
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1)
            };

            _mockUserRepository.Setup(r => r.FindByPasswordResetTokenAsync("validtoken"))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User account is no longer active.", result.Error);
            Assert.Equal(StatusCodes.Status410Gone, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnSuccess_WhenValidTokenAndPassword()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Token = "validtoken",
                NewPassword = "newpassword123",
                ConfirmNewPassword = "newpassword123"
            };

            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "oldhashedpassword",
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow,
                PasswordResetToken = "validtoken",
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1)
            };

            _mockUserRepository.Setup(r => r.FindByPasswordResetTokenAsync("validtoken"))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Password has been reset successfully.", result.Value?.Message);
            Assert.True(result.Value?.ResetAt > DateTime.UtcNow.AddMinutes(-1));
            
            // Verify password was hashed and reset token was cleared
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => 
                u.Password != "oldhashedpassword" &&
                u.PasswordResetToken == null &&
                u.PasswordResetTokenExpiry == null &&
                u.Lastupdatedat != null)), Times.Once);
        }
    }
}