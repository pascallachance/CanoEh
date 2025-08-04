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
    public class RestoreUserShould
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserService _userService;

        public RestoreUserShould()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task ReturnFailure_WhenTokenIsEmpty()
        {
            // Arrange
            var request = new RestoreUserRequest { Token = "" };

            // Act
            var result = await _userService.RestoreUserAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Restore token is required.", result.Error);
            Assert.Equal(400, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenTokenIsNull()
        {
            // Arrange
            var request = new RestoreUserRequest { Token = null };

            // Act
            var result = await _userService.RestoreUserAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Restore token is required.", result.Error);
            Assert.Equal(400, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenTokenIsInvalidOrExpired()
        {
            // Arrange
            var request = new RestoreUserRequest { Token = "invalid-token" };
            
            _mockUserRepository.Setup(repo => repo.FindByRestoreUserTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.RestoreUserAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Invalid or expired restore token.", result.Error);
            Assert.Equal(404, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenRestoreOperationFails()
        {
            // Arrange
            var request = new RestoreUserRequest { Token = "valid-token" };
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
                Createdat = DateTime.UtcNow,
                RestoreUserToken = "valid-token",
                RestoreUserTokenExpiry = DateTime.UtcNow.AddHours(1) // Valid for 1 more hour
            };

            _mockUserRepository.Setup(repo => repo.FindByRestoreUserTokenAsync(request.Token))
                .ReturnsAsync(deletedUser);
            _mockUserRepository.Setup(repo => repo.RestoreUserByTokenAsync(request.Token))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.RestoreUserAsync(request);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Failed to restore user account.", result.Error);
            Assert.Equal(500, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnSuccess_WhenValidTokenAndRestoreSucceeds()
        {
            // Arrange
            var request = new RestoreUserRequest { Token = "valid-token" };
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
                Createdat = DateTime.UtcNow,
                RestoreUserToken = "valid-token",
                RestoreUserTokenExpiry = DateTime.UtcNow.AddHours(1) // Valid for 1 more hour
            };

            _mockUserRepository.Setup(repo => repo.FindByRestoreUserTokenAsync(request.Token))
                .ReturnsAsync(deletedUser);
            _mockUserRepository.Setup(repo => repo.RestoreUserByTokenAsync(request.Token))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.RestoreUserAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("deleteduser", result.Value!.Email);
            Assert.Contains("successfully restored", result.Value.Message);
            
            // Verify that restore was called
            _mockUserRepository.Verify(repo => repo.RestoreUserByTokenAsync(
                It.Is<string>(token => token == request.Token)), Times.Once);
        }

        [Fact]
        public async Task ReturnSuccess_WhenCalledWithWhitespaceAroundToken()
        {
            // Arrange
            var request = new RestoreUserRequest { Token = "  valid-token  " };
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
                Createdat = DateTime.UtcNow,
                RestoreUserToken = "valid-token",
                RestoreUserTokenExpiry = DateTime.UtcNow.AddHours(1)
            };

            _mockUserRepository.Setup(repo => repo.FindByRestoreUserTokenAsync("  valid-token  "))
                .ReturnsAsync(deletedUser);
            _mockUserRepository.Setup(repo => repo.RestoreUserByTokenAsync("  valid-token  "))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.RestoreUserAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("deleteduser", result.Value!.Email);
        }

        [Fact]
        public async Task CallRepositoryWithCorrectParameters()
        {
            // Arrange
            var token = "test-token-123";
            var request = new RestoreUserRequest { Token = token };
            var deletedUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Deleted = true,
                ValidEmail = true,
                Createdat = DateTime.UtcNow,
                RestoreUserToken = token,
                RestoreUserTokenExpiry = DateTime.UtcNow.AddDays(1)
            };

            _mockUserRepository.Setup(repo => repo.FindByRestoreUserTokenAsync(token))
                .ReturnsAsync(deletedUser);
            _mockUserRepository.Setup(repo => repo.RestoreUserByTokenAsync(token))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.RestoreUserAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            
            // Verify the correct token was used for both calls
            _mockUserRepository.Verify(repo => repo.FindByRestoreUserTokenAsync(
                It.Is<string>(t => t == token)), Times.Once);
            _mockUserRepository.Verify(repo => repo.RestoreUserByTokenAsync(
                It.Is<string>(t => t == token)), Times.Once);
        }
    }
}