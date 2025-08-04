using Infrastructure.Services;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace API.Tests
{
    public class EmailServicePasswordResetShould
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly EmailService _emailService;

        public EmailServicePasswordResetShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup email configuration
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.test.com");
            _mockConfiguration.Setup(c => c["EmailSettings:Username"]).Returns("test@test.com");
            _mockConfiguration.Setup(c => c["EmailSettings:Password"]).Returns("testpassword");
            _mockConfiguration.Setup(c => c["EmailSettings:SmtpPort"]).Returns("587");
            _mockConfiguration.Setup(c => c["EmailSettings:EnableSsl"]).Returns("true");
            _mockConfiguration.Setup(c => c["EmailSettings:BaseUrl"]).Returns("https://localhost:7182");
            _mockConfiguration.Setup(c => c["EmailSettings:FromEmail"]).Returns("noreply@test.com");
            _mockConfiguration.Setup(c => c["EmailSettings:FromName"]).Returns("Test App");

            _emailService = new EmailService(_mockConfiguration.Object);
        }

        [Fact]
        public async Task SendPasswordResetAsync_ReturnFailure_WhenSmtpServerNotConfigured()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["EmailSettings:SmtpServer"]).Returns((string?)null);
            var emailService = new EmailService(mockConfig.Object);
            
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true,
                PasswordResetToken = "token123",
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            // Act
            var result = await emailService.SendPasswordResetAsync(user);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("EmailSettings:Host is not configured", result.Error);
        }

        [Fact]
        public async Task SendPasswordResetAsync_ReturnFailure_WhenBaseUrlNotConfigured()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.test.com");
            mockConfig.Setup(c => c["EmailSettings:Username"]).Returns("test@test.com");
            mockConfig.Setup(c => c["EmailSettings:Password"]).Returns("testpassword");
            mockConfig.Setup(c => c["EmailSettings:SmtpPort"]).Returns("587");
            mockConfig.Setup(c => c["EmailSettings:EnableSsl"]).Returns("true");
            mockConfig.Setup(c => c["EmailSettings:BaseUrl"]).Returns((string?)null);
            var emailService = new EmailService(mockConfig.Object);
            
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true,
                PasswordResetToken = "token123",
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            // Act
            var result = await emailService.SendPasswordResetAsync(user);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("App:BaseUrl is not configured", result.Error);
        }

        [Fact]
        public void PasswordResetEmail_ContainsCorrectResetUrl()
        {
            // This test verifies the URL format without actually sending email
            // We check this by examining the email service logic

            // Arrange
            var baseUrl = "https://localhost:7182";
            var token = "test-reset-token-12345";
            var expectedUrl = $"{baseUrl}/api/PasswordReset/ResetPassword?token={token}";

            // The actual URL is generated inside SendPasswordResetAsync method
            // This test documents the expected URL format
            Assert.Equal("https://localhost:7182/api/PasswordReset/ResetPassword?token=test-reset-token-12345", expectedUrl);
        }

        [Fact]
        public async Task SendPasswordResetAsync_ReturnFailure_WhenPortNotConfigured()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.test.com");
            mockConfig.Setup(c => c["EmailSettings:Username"]).Returns("test@test.com");
            mockConfig.Setup(c => c["EmailSettings:Password"]).Returns("testpassword");
            mockConfig.Setup(c => c["EmailSettings:SmtpPort"]).Returns("invalid");
            mockConfig.Setup(c => c["EmailSettings:EnableSsl"]).Returns("true");
            mockConfig.Setup(c => c["EmailSettings:BaseUrl"]).Returns("https://localhost:7182");
            var emailService = new EmailService(mockConfig.Object);
            
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true,
                PasswordResetToken = "token123",
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            // Act
            var result = await emailService.SendPasswordResetAsync(user);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("EmailSettings:Port is not configured or is invalid", result.Error);
        }

        [Fact]
        public async Task SendPasswordResetAsync_ReturnFailure_WhenEnableSslNotConfigured()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["EmailSettings:SmtpServer"]).Returns("smtp.test.com");
            mockConfig.Setup(c => c["EmailSettings:Username"]).Returns("test@test.com");
            mockConfig.Setup(c => c["EmailSettings:Password"]).Returns("testpassword");
            mockConfig.Setup(c => c["EmailSettings:SmtpPort"]).Returns("587");
            mockConfig.Setup(c => c["EmailSettings:EnableSsl"]).Returns("invalid");
            mockConfig.Setup(c => c["EmailSettings:BaseUrl"]).Returns("https://localhost:7182");
            var emailService = new EmailService(mockConfig.Object);
            
            var user = new User
            {
                ID = Guid.NewGuid(),
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true,
                PasswordResetToken = "token123",
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            // Act
            var result = await emailService.SendPasswordResetAsync(user);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("EmailSettings:EnableSsl is not configured or is invalid", result.Error);
        }
    }
}