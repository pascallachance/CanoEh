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
                Email = "testuser@test.com",
                Password = new Helpers.Common.PasswordHasher().HashPassword("password123"),
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = false // Email not validated
            ,
                Firstname = "Test",
                Lastname = "Test"};

            mockRepo.Setup(repo => repo.FindByEmailAsync("test@example.com"))
                    .ReturnsAsync(existingUser);

            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            var loginService = new LoginService(mockRepo.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "password123" };

            // Act
            var result = await loginService.LoginAsync(loginRequest, null, null);

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
                Email = "test@example.com",
                Password = new Helpers.Common.PasswordHasher().HashPassword("password123"),
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true // Email validated
            ,
                Firstname = "Test",
                Lastname = "Test"};

            mockRepo.Setup(repo => repo.FindByEmailAsync("test@example.com"))
                    .ReturnsAsync(existingUser);

            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            // Setup session service to return successful session creation
            var session = new Session 
            { 
                SessionId = Guid.NewGuid(), 
                UserId = existingUser.ID, 
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            mockSessionService.Setup(s => s.CreateSessionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(Result.Success(session));
            
            var loginService = new LoginService(mockRepo.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "password123" };

            // Act
            var result = await loginService.LoginAsync(loginRequest, null, null);

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
                Email = "test@example.com",
                Password = new Helpers.Common.PasswordHasher().HashPassword("password123"),
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = false,
                EmailValidationToken = validationToken
            ,
                Firstname = "Test",
                Lastname = "Test"};

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