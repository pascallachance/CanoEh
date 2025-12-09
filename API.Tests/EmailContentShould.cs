using Helpers.Common;
using Xunit;

namespace API.Tests
{
    public class EmailContentShould
    {
        [Fact]
        public void GetEmailValidation_ReturnEnglishContent_WhenLanguageIsEn()
        {
            // Arrange
            var firstname = "John";
            var lastname = "Doe";
            var validationUrl = "https://example.com/validate/token123";
            var language = "en";

            // Act
            var (subject, body) = EmailContent.GetEmailValidation(firstname, lastname, validationUrl, language);

            // Assert
            Assert.Equal("Email Validation", subject);
            Assert.Contains("Hello John Doe", body);
            Assert.Contains("Thank you for registering with CanoEh!", body);
            Assert.Contains(validationUrl, body);
            Assert.Contains("The CanoEh Team", body);
        }

        [Fact]
        public void GetEmailValidation_ReturnFrenchContent_WhenLanguageIsFr()
        {
            // Arrange
            var firstname = "Jean";
            var lastname = "Dupont";
            var validationUrl = "https://example.com/validate/token456";
            var language = "fr";

            // Act
            var (subject, body) = EmailContent.GetEmailValidation(firstname, lastname, validationUrl, language);

            // Assert
            Assert.Equal("Validation de l'adresse e-mail", subject);
            Assert.Contains("Bonjour Jean Dupont", body);
            Assert.Contains("Merci de vous être inscrit à CanoEh!", body);
            Assert.Contains(validationUrl, body);
            Assert.Contains("L'équipe CanoEh", body);
        }

        [Fact]
        public void GetEmailValidation_ReturnEnglishContent_WhenLanguageIsNull()
        {
            // Arrange
            var firstname = "Test";
            var lastname = "User";
            var validationUrl = "https://example.com/validate/token789";
            string? language = null;

            // Act
            var (subject, body) = EmailContent.GetEmailValidation(firstname, lastname, validationUrl, language);

            // Assert
            Assert.Equal("Email Validation", subject);
            Assert.Contains("Hello Test User", body);
            Assert.Contains("Thank you for registering with CanoEh!", body);
        }

        [Fact]
        public void GetPasswordReset_ReturnEnglishContent_WhenLanguageIsEn()
        {
            // Arrange
            var firstname = "Alice";
            var lastname = "Smith";
            var resetUrl = "https://example.com/reset/token123";
            var language = "en";

            // Act
            var (subject, body) = EmailContent.GetPasswordReset(firstname, lastname, resetUrl, language);

            // Assert
            Assert.Equal("Password Reset Request", subject);
            Assert.Contains("Hello Alice Smith", body);
            Assert.Contains("You have requested to reset your password", body);
            Assert.Contains(resetUrl, body);
            Assert.Contains("This link will expire in 24 hours", body);
        }

        [Fact]
        public void GetPasswordReset_ReturnFrenchContent_WhenLanguageIsFr()
        {
            // Arrange
            var firstname = "Marie";
            var lastname = "Martin";
            var resetUrl = "https://example.com/reset/token456";
            var language = "fr";

            // Act
            var (subject, body) = EmailContent.GetPasswordReset(firstname, lastname, resetUrl, language);

            // Assert
            Assert.Equal("Demande de réinitialisation du mot de passe", subject);
            Assert.Contains("Bonjour Marie Martin", body);
            Assert.Contains("Vous avez demandé de réinitialiser votre mot de passe", body);
            Assert.Contains(resetUrl, body);
            Assert.Contains("Ce lien expirera dans 24 heures", body);
        }

        [Fact]
        public void GetRestoreUser_ReturnEnglishContent_WhenLanguageIsEn()
        {
            // Arrange
            var firstname = "Bob";
            var lastname = "Johnson";
            var restoreUrl = "https://example.com/restore/token123";
            var language = "en";

            // Act
            var (subject, body) = EmailContent.GetRestoreUser(firstname, lastname, restoreUrl, language);

            // Assert
            Assert.Equal("Account Restoration Request", subject);
            Assert.Contains("Hello Bob Johnson", body);
            Assert.Contains("You have requested to restore your deleted CanoEh account", body);
            Assert.Contains(restoreUrl, body);
        }

        [Fact]
        public void GetRestoreUser_ReturnFrenchContent_WhenLanguageIsFr()
        {
            // Arrange
            var firstname = "Sophie";
            var lastname = "Dubois";
            var restoreUrl = "https://example.com/restore/token456";
            var language = "fr";

            // Act
            var (subject, body) = EmailContent.GetRestoreUser(firstname, lastname, restoreUrl, language);

            // Assert
            Assert.Equal("Demande de restauration de compte", subject);
            Assert.Contains("Bonjour Sophie Dubois", body);
            Assert.Contains("Vous avez demandé de restaurer votre compte CanoEh supprimé", body);
            Assert.Contains(restoreUrl, body);
        }

        [Fact]
        public void GetEmailValidation_ReturnEnglishContent_WhenLanguageIsUnknown()
        {
            // Arrange
            var firstname = "Test";
            var lastname = "User";
            var validationUrl = "https://example.com/validate/token999";
            var language = "es"; // Spanish - not supported, should default to English

            // Act
            var (subject, body) = EmailContent.GetEmailValidation(firstname, lastname, validationUrl, language);

            // Assert
            Assert.Equal("Email Validation", subject);
            Assert.Contains("Hello Test User", body);
            Assert.Contains("Thank you for registering with CanoEh!", body);
        }

        [Theory]
        [InlineData("en")]
        [InlineData("EN")]
        [InlineData("En")]
        public void GetEmailValidation_BeCaseInsensitive_ForLanguageCode(string language)
        {
            // Arrange
            var firstname = "Test";
            var lastname = "User";
            var validationUrl = "https://example.com/validate/token123";

            // Act
            var (subject, body) = EmailContent.GetEmailValidation(firstname, lastname, validationUrl, language);

            // Assert
            Assert.Equal("Email Validation", subject);
            Assert.Contains("Hello Test User", body);
        }

        [Theory]
        [InlineData("fr")]
        [InlineData("FR")]
        [InlineData("Fr")]
        public void GetPasswordReset_BeCaseInsensitive_ForFrenchLanguageCode(string language)
        {
            // Arrange
            var firstname = "Test";
            var lastname = "User";
            var resetUrl = "https://example.com/reset/token123";

            // Act
            var (subject, body) = EmailContent.GetPasswordReset(firstname, lastname, resetUrl, language);

            // Assert
            Assert.Equal("Demande de réinitialisation du mot de passe", subject);
            Assert.Contains("Bonjour Test User", body);
        }
    }
}
