namespace Domain.Models.Responses
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public Guid? SessionId { get; set; }
    }
}
