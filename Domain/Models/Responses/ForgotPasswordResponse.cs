namespace Domain.Models.Responses
{
    public class ForgotPasswordResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = "If the email address exists in our system, you will receive a password reset link shortly.";
    }
}