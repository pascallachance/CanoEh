using Helpers.Common;

namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task<Result> SendEmailValidationAsync(string email, string validationToken);
        Task<Result> SendPasswordResetAsync(string email, string resetToken);
        Task<Result> SendRestoreUserEmailAsync(string email, string restoreToken);
    }
}