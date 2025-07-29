using Helpers.Common;

namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task<Result> SendEmailValidationAsync(string email, string username, string validationToken);
    }
}