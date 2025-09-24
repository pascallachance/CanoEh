namespace Domain.Models.Responses
{
    public class ForgotPasswordResponse
    {
        // Default message constants for better maintainability and locale support
        public static class DefaultMessages
        {
            public const string ForgotPasswordMessage = "If the email address exists in our system, you will receive a password reset link shortly.";
            public const string ForgotPasswordMessageFr = "Si l'adresse e-mail existe dans notre système, vous recevrez bientôt un lien de réinitialisation du mot de passe.";
            
            /// <summary>
            /// Get localized message based on locale parameter
            /// </summary>
            /// <param name="locale">Locale code (e.g., "en", "fr")</param>
            /// <returns>Localized message or English as fallback</returns>
            public static string GetLocalizedMessage(string? locale)
            {
                return locale?.ToLowerInvariant() switch
                {
                    "fr" => ForgotPasswordMessageFr,
                    _ => ForgotPasswordMessage // Default to English for unknown/null locales
                };
            }
        }

        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = DefaultMessages.ForgotPasswordMessage;

        /// <summary>
        /// Default constructor with standard message
        /// </summary>
        public ForgotPasswordResponse() { }

        /// <summary>
        /// Constructor allowing custom message for configurability
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="message">Custom message (optional, defaults to standard message)</param>
        public ForgotPasswordResponse(string email, string? message = null)
        {
            Email = email;
            Message = message ?? DefaultMessages.ForgotPasswordMessage;
        }

        /// <summary>
        /// Creates a ForgotPasswordResponse with locale-aware message
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="locale">Locale code for message localization</param>
        /// <param name="customMessage">Custom message that overrides locale (optional)</param>
        /// <returns>ForgotPasswordResponse with appropriate localized message</returns>
        public static ForgotPasswordResponse CreateWithLocale(string email, string? locale, string? customMessage = null)
        {
            return new ForgotPasswordResponse
            {
                Email = email,
                Message = customMessage ?? DefaultMessages.GetLocalizedMessage(locale)
            };
        }
    }
}