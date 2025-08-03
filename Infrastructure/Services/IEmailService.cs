using Helpers.Common;

namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task<Result> SendEmailValidationAsync(string email, string firstname, string lastname, string validationToken);
        Task<Result> SendPasswordResetAsync(string email, string firstname, string lastname, string resetToken);
        Task<Result> SendRestoreUserEmailAsync(string email, string firstname, string lastname, string restoreToken);
    }
}