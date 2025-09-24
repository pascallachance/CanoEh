namespace Domain.Models.Responses
{
    public class ForgotPasswordResponse
    {
        // Default message constant for better maintainability
        public static class DefaultMessages
        {
            public const string ForgotPasswordMessage = "If the email address exists in our system, you will receive a password reset link shortly.";
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
    }
}