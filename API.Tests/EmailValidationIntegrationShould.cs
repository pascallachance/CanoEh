using Domain.Models.Requests;
using Domain.Services.Implementations;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class EmailValidationIntegrationShould
    {
        [Fact]
        public async Task BlockLogin_WhenEmailNotValidated()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            
            var existingUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Password = new Helpers.Common.PasswordHasher().HashPassword("password123"),
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = false // Email not validated
            };

            mockRepo.Setup(repo => repo.FindByUsernameAsync("testuser"))
                    .ReturnsAsync(existingUser);

            var loginService = new LoginService(mockRepo.Object, mockEmailService.Object);
            var loginRequest = new LoginRequest { Username = "testuser", Password = "password123" };

            // Act
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status403Forbidden, result.ErrorCode);
            Assert.Equal("Please validate your email address before logging in", result.Error);
        }

        [Fact]
        public async Task AllowLogin_WhenEmailIsValidated()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            
            var existingUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Password = new Helpers.Common.PasswordHasher().HashPassword("password123"),
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true // Email validated
            };

            mockRepo.Setup(repo => repo.FindByUsernameAsync("testuser"))
                    .ReturnsAsync(existingUser);

            var loginService = new LoginService(mockRepo.Object, mockEmailService.Object);
            var loginRequest = new LoginRequest { Username = "testuser", Password = "password123" };

            // Act
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task ValidateEmail_ChangesValidEmailToTrue()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            
            var validationToken = "test-validation-token";
            var existingUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Password = new Helpers.Common.PasswordHasher().HashPassword("password123"),
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = false,
                EmailValidationToken = validationToken
            };

            User? updatedUser = null;
            mockRepo.Setup(repo => repo.FindByEmailValidationTokenAsync(validationToken))
                    .ReturnsAsync(existingUser);
            mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                    .ReturnsAsync((User u) => {
                        updatedUser = u;
                        return u;
                    });

            var userService = new UserService(mockRepo.Object, mockEmailService.Object);

            // Act
            var result = await userService.ValidateEmailByTokenAsync(validationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(updatedUser);
            Assert.True(updatedUser.ValidEmail);
            Assert.Null(updatedUser.EmailValidationToken); // Token should be cleared after use
        }
    }
}