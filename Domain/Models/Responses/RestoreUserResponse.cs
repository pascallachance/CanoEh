namespace Domain.Models.Responses
{
    public class RestoreUserResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = "Your account has been successfully restored.";
    }
}