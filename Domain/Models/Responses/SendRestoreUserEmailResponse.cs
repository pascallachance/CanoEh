namespace Domain.Models.Responses
{
    public class SendRestoreUserEmailResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = "If the email address corresponds to a deleted account, you will receive a restore account link shortly.";
    }
}