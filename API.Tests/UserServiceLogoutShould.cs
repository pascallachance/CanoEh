using Domain.Services.Implementations;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class UserServiceLogoutShould
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UserService _userService;

        public UserServiceLogoutShould()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task ReturnSuccess_WhenUserLoggedOutSuccessfully()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = email,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow.AddDays(-1),
                Lastlogin = DateTime.UtcNow.AddMinutes(-30),
                Lastlogout = null
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _userService.LogoutAsync(email);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);

            // Verify that the user's logout date was updated
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => 
                u.Lastlogout.HasValue && 
                u.Lastupdatedat.HasValue &&
                u.Email == email)), Times.Once);
        }

        [Fact]
        public async Task ReturnFailure_WhenUsernameIsEmpty()
        {
            // Act
            var result = await _userService.LogoutAsync("");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);

            _mockUserRepository.Verify(r => r.FindByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ReturnFailure_WhenUsernameIsNull()
        {
            // Act
            var result = await _userService.LogoutAsync(null!);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);

            _mockUserRepository.Verify(r => r.FindByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ReturnFailure_WhenUsernameIsWhitespace()
        {
            // Act
            var result = await _userService.LogoutAsync("   ");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);

            _mockUserRepository.Verify(r => r.FindByEmailAsync(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUserRepository.Setup(r => r.FindByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.LogoutAsync(email);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User not found.", result.Error);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);

            _mockUserRepository.Verify(r => r.FindByEmailAsync(email), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ReturnFailure_WhenUserIsDeleted()
        {
            // Arrange
            var email = "deleted@example.com";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = "deleteduser",
                Firstname = "Deleted",
                Lastname = "User",
                Email = email,
                Password = "hashedpassword",
                Deleted = true, // User is deleted
                ValidEmail = true,
                Createdat = DateTime.UtcNow.AddDays(-1)
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _userService.LogoutAsync(email);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User is deleted.", result.Error);
            Assert.Equal(StatusCodes.Status410Gone, result.ErrorCode);

            _mockUserRepository.Verify(r => r.FindByEmailAsync(email), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateLastlogoutAndLastupdatedat_WhenLogoutSuccessful()
        {
            // Arrange
            var email = "test@example.com";
            var beforeLogout = DateTime.UtcNow;
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = email,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow.AddDays(-1),
                Lastlogin = DateTime.UtcNow.AddMinutes(-30),
                Lastlogout = null,
                Lastupdatedat = DateTime.UtcNow.AddHours(-1)
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _userService.LogoutAsync(email);
            var afterLogout = DateTime.UtcNow;

            // Assert
            Assert.True(result.IsSuccess);

            // Verify both Lastlogout and Lastupdatedat were set to recent times
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => 
                u.Lastlogout.HasValue && 
                u.Lastlogout >= beforeLogout && 
                u.Lastlogout <= afterLogout &&
                u.Lastupdatedat.HasValue && 
                u.Lastupdatedat >= beforeLogout && 
                u.Lastupdatedat <= afterLogout)), Times.Once);
        }
    }
}