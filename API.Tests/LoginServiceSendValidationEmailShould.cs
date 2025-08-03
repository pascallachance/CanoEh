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
            var email = "test@example.com";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = email,
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = false,
                EmailValidationToken = null
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            _mockEmailService.Setup(e => e.SendEmailValidationAsync(It.IsAny<User>()))
                              .ReturnsAsync(Result.Success());

            // Act
            var result = await _loginService.SendValidationEmailAsync(email);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => !string.IsNullOrEmpty(u.EmailValidationToken))), Times.Once);
            _mockEmailService.Verify(e => e.SendEmailValidationAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailIsEmpty()
        {
            // Act
            var result = await _loginService.SendValidationEmailAsync("");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync((User?)null);

            // Act
            var result = await _loginService.SendValidationEmailAsync(email);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User not found.", result.Error);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenUserIsDeleted()
        {
            // Arrange
            var email = "deleted@example.com";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = email,
                Firstname = "Deleted",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = true
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);

            // Act
            var result = await _loginService.SendValidationEmailAsync(email);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("User account is deleted.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailAlreadyValidated()
        {
            // Arrange
            var email = "validated@example.com";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = email,
                Firstname = "Validated",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = true,
                Deleted = false
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);

            // Act
            var result = await _loginService.SendValidationEmailAsync(email);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is already validated.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailServiceFails()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = email,
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = false,
                EmailValidationToken = null
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            _mockEmailService.Setup(e => e.SendEmailValidationAsync(It.IsAny<User>()))
                             .ReturnsAsync(Result.Failure("Failed to send validation email."));

            // Act
            var result = await _loginService.SendValidationEmailAsync(email);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Failed to send validation email.", result.Error);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }

        [Fact]
        public async Task ReturnFailure_WhenEmailServiceThrowsException()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = email,
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                ValidEmail = false,
                Deleted = false,
                EmailValidationToken = null
            };

            _mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            _mockEmailService.Setup(e => e.SendEmailValidationAsync(It.IsAny<User>()))
                           .ThrowsAsync(new Exception("SMTP error"));

            // Act
            var result = await _loginService.SendValidationEmailAsync(email);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Error sending validation email: SMTP error", result.Error);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
        }
    }
}