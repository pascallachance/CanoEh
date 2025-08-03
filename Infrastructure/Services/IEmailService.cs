using Helpers.Common;
using Infrastructure.Data;

namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task<Result> SendEmailValidationAsync(User user);
        Task<Result> SendPasswordResetAsync(User user);
        Task<Result> SendRestoreUserEmailAsync(User user);
    }
}