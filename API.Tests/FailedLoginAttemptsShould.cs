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
    public class FailedLoginAttemptsShould
    {
        [Fact]
        public async Task IncrementFailedLoginAttempts_WhenPasswordIsIncorrect()
        {
            // Arrange
            var email = "user@example.com";
            var correctPassword = "password123";
            var incorrectPassword = "wrongpassword";
            var hasher = new PasswordHasher();
            
            var user = new User
            {
                ID = Guid.NewGuid(),
                Firstname = "Test",
                Lastname = "User", 
                Email = email,
                Password = hasher.HashPassword(correctPassword),
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                LastFailedLoginAttempt = null
            };

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            
            mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);

            var loginService = new LoginService(mockUserRepository.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = incorrectPassword
            };

            // Act
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(401, result.ErrorCode);
            Assert.Equal("Invalid email or password", result.Error);
            
            // Verify that UpdateAsync was called to increment failed login attempts
            mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => 
                u.FailedLoginAttempts == 1 && 
                u.LastFailedLoginAttempt != null)), Times.Once);
        }

        [Fact]
        public async Task ResetFailedLoginAttempts_WhenLoginIsSuccessful()
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
                Createdat = DateTime.UtcNow,
                FailedLoginAttempts = 2,
                LastFailedLoginAttempt = DateTime.UtcNow.AddMinutes(-5)
            };

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            
            mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            
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
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsSuccess);
            
            // Verify that UpdateAsync was called to reset failed login attempts
            mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => 
                u.FailedLoginAttempts == 0 && 
                u.LastFailedLoginAttempt == null)), Times.Once);
        }

        [Fact]
        public async Task BlockLogin_WhenThreeFailedAttemptsWithin10Minutes()
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
                Createdat = DateTime.UtcNow,
                FailedLoginAttempts = 3,
                LastFailedLoginAttempt = DateTime.UtcNow.AddMinutes(-5) // 5 minutes ago
            };

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);

            var loginService = new LoginService(mockUserRepository.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(429, result.ErrorCode); // Too Many Requests
            Assert.Contains("Account is locked", result.Error);
            Assert.Contains("too many failed login attempts", result.Error);
            
            // Verify that UpdateAsync was NOT called because account is locked
            mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AllowLogin_WhenLockoutPeriodExpired()
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
                Createdat = DateTime.UtcNow,
                FailedLoginAttempts = 3,
                LastFailedLoginAttempt = DateTime.UtcNow.AddMinutes(-11) // 11 minutes ago (lockout expired)
            };

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            
            mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => u);
            
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
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsSuccess);
            
            // Verify that UpdateAsync was called once to reset failed login attempts
            // (optimization: consolidates lockout expiry reset and successful login reset)
            mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => 
                u.FailedLoginAttempts == 0 && 
                u.LastFailedLoginAttempt == null)), Times.Once);
        }

        [Fact]
        public async Task NotIncrementFailedLoginAttempts_ForNonExistentUser()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password123";

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync((User?)null);

            var loginService = new LoginService(mockUserRepository.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(401, result.ErrorCode);
            Assert.Equal("Invalid email or password", result.Error);
            
            // Verify that UpdateAsync was NOT called for non-existent user
            mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task NotIncrementFailedLoginAttempts_ForUnvalidatedEmail()
        {
            // Arrange
            var email = "unvalidated@example.com";
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
                ValidEmail = false, // Email not validated
                Createdat = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                LastFailedLoginAttempt = null
            };

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);

            var loginService = new LoginService(mockUserRepository.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await loginService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(403, result.ErrorCode); // Forbidden - email not validated
            Assert.Contains("validate your email", result.Error);
            
            // Verify that UpdateAsync was NOT called - unvalidated emails don't accumulate failed attempts
            mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AccumulateFailedLoginAttempts_OverMultipleAttempts()
        {
            // Arrange
            var email = "user@example.com";
            var correctPassword = "password123";
            var incorrectPassword = "wrongpassword";
            var hasher = new PasswordHasher();
            
            var user = new User
            {
                ID = Guid.NewGuid(),
                Firstname = "Test",
                Lastname = "User", 
                Email = email,
                Password = hasher.HashPassword(correctPassword),
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                LastFailedLoginAttempt = null
            };

            var mockUserRepository = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var mockSessionService = new Mock<ISessionService>();
            var mockUserService = new Mock<IUserService>();
            
            mockUserRepository.Setup(r => r.FindByEmailAsync(email))
                             .ReturnsAsync(user);
            
            mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                             .ReturnsAsync((User u) => 
                             {
                                 user.FailedLoginAttempts = u.FailedLoginAttempts;
                                 user.LastFailedLoginAttempt = u.LastFailedLoginAttempt;
                                 return u;
                             });

            var loginService = new LoginService(mockUserRepository.Object, mockEmailService.Object, mockSessionService.Object, mockUserService.Object);

            // Act - First failed attempt
            var loginRequest1 = new LoginRequest { Email = email, Password = incorrectPassword };
            var result1 = await loginService.LoginAsync(loginRequest1);

            // Assert first attempt
            Assert.True(result1.IsFailure);
            Assert.Equal(1, user.FailedLoginAttempts);

            // Act - Second failed attempt
            var loginRequest2 = new LoginRequest { Email = email, Password = incorrectPassword };
            var result2 = await loginService.LoginAsync(loginRequest2);

            // Assert second attempt
            Assert.True(result2.IsFailure);
            Assert.Equal(2, user.FailedLoginAttempts);

            // Act - Third failed attempt
            var loginRequest3 = new LoginRequest { Email = email, Password = incorrectPassword };
            var result3 = await loginService.LoginAsync(loginRequest3);

            // Assert third attempt
            Assert.True(result3.IsFailure);
            Assert.Equal(3, user.FailedLoginAttempts);

            // Act - Fourth attempt should be blocked
            var loginRequest4 = new LoginRequest { Email = email, Password = correctPassword };
            var result4 = await loginService.LoginAsync(loginRequest4);

            // Assert fourth attempt is blocked even with correct password
            Assert.True(result4.IsFailure);
            Assert.Equal(429, result4.ErrorCode);
            Assert.Contains("Account is locked", result4.Error);
        }
    }
}
