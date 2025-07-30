using Helpers.Common;

namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task<Result> SendEmailValidationAsync(string email, string username, string validationToken);
        Task<Result> SendPasswordResetAsync(string email, string username, string resetToken);
    }
}