using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Implementations;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Moq;
using Xunit;

namespace API.Tests
{
    public class SendRestoreUserEmailShould
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserService _userService;

        public SendRestoreUserEmailShould()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailIsEmpty()
        {
            // Arrange
            var request = new SendRestoreUserEmailRequest { Email = "" };

            // Act
            var result = await _userService.SendRestoreUserEmailAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(400, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailIsNull()
        {
            // Arrange
            var request = new SendRestoreUserEmailRequest { Email = null };

            // Act
            var result = await _userService.SendRestoreUserEmailAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(400, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailFormatIsInvalid()
        {
            // Arrange
            var request = new SendRestoreUserEmailRequest { Email = "invalid-email" };

            // Act
            var result = await _userService.SendRestoreUserEmailAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Invalid email format.", result.Error);
            Assert.Equal(400, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnSuccess_WhenEmailDoesNotExist()
        {
            // Arrange
            var request = new SendRestoreUserEmailRequest { Email = "nonexistent@example.com" };
            
            _mockUserRepository.Setup(repo => repo.FindDeletedByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.SendRestoreUserEmailAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("nonexistent@example.com", result.Value!.Email);
            Assert.Contains("deleted account", result.Value.Message);
        }

        [Fact]
        public async Task ReturnSuccess_WhenDeletedUserExistsAndEmailSentSuccessfully()
        {
            // Arrange
            var request = new SendRestoreUserEmailRequest { Email = "deleted@example.com" };
            var deletedUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "deleteduser",
                Email = "deleted@example.com",
                Firstname = "John",
                Lastname = "Doe",
                Password = "hashedpassword",
                Deleted = true,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            _mockUserRepository.Setup(repo => repo.FindDeletedByEmailAsync(request.Email))
                .ReturnsAsync(deletedUser);
            _mockUserRepository.Setup(repo => repo.UpdateRestoreUserTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            _mockEmailService.Setup(service => service.SendRestoreUserEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _userService.SendRestoreUserEmailAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("deleted@example.com", result.Value!.Email);
            
            // Verify token was updated
            _mockUserRepository.Verify(repo => repo.UpdateRestoreUserTokenAsync(
                It.Is<string>(email => email == request.Email),
                It.IsAny<string>(),
                It.IsAny<DateTime>()), Times.Once);
            
            // Verify email was sent
            _mockEmailService.Verify(service => service.SendRestoreUserEmailAsync(
                It.Is<string>(email => email == request.Email),
                It.Is<string>(username => username == deletedUser.Uname),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ReturnSuccess_WhenDeletedUserExistsButEmailSendingFails()
        {
            // Arrange
            var request = new SendRestoreUserEmailRequest { Email = "deleted@example.com" };
            var deletedUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "deleteduser",
                Email = "deleted@example.com",
                Firstname = "John",
                Lastname = "Doe",
                Password = "hashedpassword",
                Deleted = true,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            _mockUserRepository.Setup(repo => repo.FindDeletedByEmailAsync(request.Email))
                .ReturnsAsync(deletedUser);
            _mockUserRepository.Setup(repo => repo.UpdateRestoreUserTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);
            _mockEmailService.Setup(service => service.SendRestoreUserEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Failure("SMTP error"));

            // Act
            var result = await _userService.SendRestoreUserEmailAsync(request);

            // Assert
            // Should still return success for security reasons (don't reveal if email exists)
            Assert.True(result.IsSuccess);
            Assert.Equal("deleted@example.com", result.Value!.Email);
        }

        [Fact]
        public async Task ReturnSuccess_WhenDeletedUserExistsButTokenUpdateFails()
        {
            // Arrange
            var request = new SendRestoreUserEmailRequest { Email = "deleted@example.com" };
            var deletedUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "deleteduser",
                Email = "deleted@example.com",
                Firstname = "John",
                Lastname = "Doe",
                Password = "hashedpassword",
                Deleted = true,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            _mockUserRepository.Setup(repo => repo.FindDeletedByEmailAsync(request.Email))
                .ReturnsAsync(deletedUser);
            _mockUserRepository.Setup(repo => repo.UpdateRestoreUserTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.SendRestoreUserEmailAsync(request);

            // Assert
            // Should still return success for security reasons
            Assert.True(result.IsSuccess);
            Assert.Equal("deleted@example.com", result.Value!.Email);
            
            // Email should not be sent if token update fails
            _mockEmailService.Verify(service => service.SendRestoreUserEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}