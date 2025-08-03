using Domain.Models.Requests;
using Domain.Services.Implementations;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Domain.Services.Interfaces;
using Moq;
using Helpers.Common;
using Xunit;

namespace API.Tests
{
    public class EmailBasedLoginIntegrationShould
    {
        [Fact]
        public async Task LoginWithEmail_Successfully()
        {
            // Arrange
            var email = "user@example.com";
            var password = "password123";
            var hasher = new PasswordHasher();
            
            var user = new User
            {
                ID = Guid.NewGuid(),
                Firstname = "Test",
                Lastname = "User", 
                Email = email,
                Password = hasher.HashPassword(password),
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            
            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = user.ID,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            
            mockSessionService.Setup(s => s.CreateSessionAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(Result.Success(session));

            var loginService = new LoginService(mockUserRepository.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await loginService.LoginAsync(loginRequest, "TestAgent", "127.0.0.1");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(session.SessionId, result.Value.SessionId);
            
            // Verify email-based lookup was used
            mockUserRepository.Verify(r => r.FindByEmailAsync(email), Times.Once);
            mockSessionService.Verify(s => s.CreateSessionAsync(user.ID, "TestAgent", "127.0.0.1"), Times.Once);
        }

        [Fact]
        public void LoginRequest_ValidatesEmailFormat()
        {
            // Arrange & Act
            var invalidEmailRequest = new LoginRequest
            {
                Email = "invalid-email",
                Password = "password123"
            };

            var result = invalidEmailRequest.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Email format is invalid", result.Error);
        }

        [Fact]
        public void LoginRequest_RequiresEmail()
        {
            // Arrange & Act
            var noEmailRequest = new LoginRequest
            {
                Email = "",
                Password = "password123"
            };

            var result = noEmailRequest.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Email is required", result.Error);
        }
    }
}