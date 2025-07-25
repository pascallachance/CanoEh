namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailValidationAsync(string email, string username, Guid userId);
    }
}