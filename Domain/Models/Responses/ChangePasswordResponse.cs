namespace Domain.Models.Responses
{
    public class ChangePasswordResponse
    {
        public string Email { get; set; } = string.Empty;
        public DateTime LastUpdatedAt { get; set; }
        public string Message { get; set; } = "Password changed successfully.";
    }
}