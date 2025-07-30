namespace Domain.Models.Responses
{
    public class ResetPasswordResponse
    {
        public string Message { get; set; } = "Password has been reset successfully.";
        public DateTime ResetAt { get; set; }
    }
}