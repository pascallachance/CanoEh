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
    public class LoginServiceSendValidationEmailShould
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ISessionService> _mockSessionService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly LoginService _loginService;

        public LoginServiceSendValidationEmailShould()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockSessionService = new Mock<ISessionService>();
            _mockUserService = new Mock<IUserService>();
            _loginService = new LoginService(_mockUserRepository.Object, _mockEmailService.Object, _mockSessionService.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task ReturnSuccess_WhenEmailSentSuccessfully()
        {
            // Arrange
            var username = "testuser";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = false,
                EmailValidationToken = null
            };

            _mockUserRepository.Setup(r => r.FindByUsernameAsync(username))
                             .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            _mockEmailService.Setup(e => e.SendEmailValidationAsync(user.Email, user.Uname, It.IsAny<string>()))
                              .ReturnsAsync(Result.Success());

            // Act
            var result = await _loginService.SendValidationEmailAsync(username);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => !string.IsNullOrEmpty(u.EmailValidationToken))), Times.Once);
            _mockEmailService.Verify(e => e.SendEmailValidationAsync(user.Email, user.Uname, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ReturnFailure_WhenUsernameIsEmpty()
        {
            // Act
            var result = await _loginService.SendValidationEmailAsync("");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Username is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var username = "nonexistentuser";
            _mockUserRepository.Setup(r => r.FindByUsernameAsync(username))
                             .ReturnsAsync((User?)null);

            // Act
            var result = await _loginService.SendValidationEmailAsync(username);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User not found.", result.Error);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenUserIsDeleted()
        {
            // Arrange
            var username = "deleteduser";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Email = "deleted@example.com",
                Firstname = "Deleted",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = true
            };

            _mockUserRepository.Setup(r => r.FindByUsernameAsync(username))
                             .ReturnsAsync(user);

            // Act
            var result = await _loginService.SendValidationEmailAsync(username);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User account is deleted.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailAlreadyValidated()
        {
            // Arrange
            var username = "validateduser";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Email = "validated@example.com",
                Firstname = "Validated",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = true,
                Deleted = false
            };

            _mockUserRepository.Setup(r => r.FindByUsernameAsync(username))
                             .ReturnsAsync(user);

            // Act
            var result = await _loginService.SendValidationEmailAsync(username);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is already validated.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailServiceFails()
        {
            // Arrange
            var username = "testuser";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = false,
                EmailValidationToken = null
            };

            _mockUserRepository.Setup(r => r.FindByUsernameAsync(username))
                             .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            _mockEmailService.Setup(e => e.SendEmailValidationAsync(user.Email, user.Uname, It.IsAny<string>()))
                             .ReturnsAsync(Result.Failure("Failed to send validation email."));

            // Act
            var result = await _loginService.SendValidationEmailAsync(username);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Failed to send validation email.", result.Error);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailServiceThrowsException()
        {
            // Arrange
            var username = "testuser";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = false,
                EmailValidationToken = null
            };

            _mockUserRepository.Setup(r => r.FindByUsernameAsync(username))
                             .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            _mockEmailService.Setup(e => e.SendEmailValidationAsync(user.Email, user.Uname, It.IsAny<string>()))
                           .ThrowsAsync(new Exception("SMTP error"));

            // Act
            var result = await _loginService.SendValidationEmailAsync(username);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Error sending validation email: SMTP error", result.Error);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }
    }
}