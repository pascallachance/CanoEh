using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace API.Tests
{
    public class ForgotPasswordShould
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserService _userService;

        public ForgotPasswordShould()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailIsEmpty()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "" };

            // Act
            var result = await _userService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailFormatIsInvalid()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "invalid-email" };

            // Act
            var result = await _userService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Invalid email format.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnSuccess_WhenEmailDoesNotExist()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };
            _mockUserRepository.Setup(r => r.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("nonexistent@example.com", result.Value?.Email);
            Assert.Contains("If the email address exists", result.Value?.Message);
        }

        [Fact]
        public async Task ReturnSuccess_WhenEmailExistsAndSendEmailSucceeds()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "test@example.com" };
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdatePasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            _mockEmailService.Setup(e => e.SendPasswordResetAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _userService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("test@example.com", result.Value?.Email);
            Assert.Contains("If the email address exists", result.Value?.Message);
            _mockUserRepository.Verify(r => r.UpdatePasswordResetTokenAsync("test@example.com", It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            _mockEmailService.Verify(e => e.SendPasswordResetAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ReturnSuccess_WhenUserIsDeleted()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "deleted@example.com" };
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = "deleteduser",
                Firstname = "Deleted",
                Lastname = "User",
                Password = "hashedpassword",
                Deleted = true,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync("deleted@example.com"))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.ForgotPasswordAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("deleted@example.com", result.Value?.Email);
            Assert.Contains("If the email address exists", result.Value?.Message);
            // Should not send email for deleted users
            _mockEmailService.Verify(e => e.SendPasswordResetAsync(It.IsAny<User>()), Times.Never);
        }
    }
}