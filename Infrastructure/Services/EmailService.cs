using System.Diagnostics;
using System.Net.Mail;
using Helpers.Common;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly string? _smtpServer = configuration["EmailSettings:SmtpServer"];
        private readonly string? _smtpUsername = configuration["EmailSettings:Username"];
        private readonly string? _smtpPassword = configuration["EmailSettings:Password"];
        private readonly string? _fromEmail = configuration["EmailSettings:FromEmail"];
        private readonly string? _fromName = configuration["EmailSettings:FromName"];
        private readonly string? _baseUrl = configuration["EmailSettings:BaseUrl"];
        private readonly int? _smtpPort = int.TryParse(configuration["EmailSettings:SmtpPort"], out var port) ? port : null;
        private readonly bool? _smtpEnableSsl = bool.TryParse(configuration["EmailSettings:EnableSsl"], out var ssl) ? ssl : null;

        public async Task<Result> SendEmailValidationAsync(User user)
        {
            // Check configuration and return error messages if missing
            if (string.IsNullOrWhiteSpace(_smtpServer))
                return Result.Failure("EmailSettings:Host is not configured.");
            if (string.IsNullOrWhiteSpace(_smtpUsername))
                return Result.Failure("EmailSettings:Username is not configured.");
            if (string.IsNullOrWhiteSpace(_smtpPassword))
                return Result.Failure("EmailSettings:Password is not configured.");
            if (string.IsNullOrWhiteSpace(_baseUrl))
                return Result.Failure("App:BaseUrl is not configured.");
            if (_smtpPort == null)
                return Result.Failure("EmailSettings:Port is not configured or is invalid.");
            if (_smtpEnableSsl == null)
                return Result.Failure("EmailSettings:EnableSsl is not configured or is invalid.");

            try
            {
                var validationUrl = $"{_baseUrl}/api/EmailValidation/ValidateEmail/{user.EmailValidationToken}";
                var body = $@"Hello {user.Firstname} {user.Lastname},

Thank you for registering with CanoEh! To complete your registration, please click the link below to validate your email address:

{validationUrl}

If you did not create this account, please ignore this email.

Best regards,
The CanoEh Team";

                using var smtp = new SmtpClient(_smtpServer, _smtpPort.Value)
                {
                    EnableSsl = _smtpEnableSsl.Value,
                    Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword)
                };
                var mail = new MailMessage(_smtpUsername, user.Email)
                {
                    From = new MailAddress(_fromEmail ?? "Unknown", _fromName),
                    Subject = "Email Validation",
                    Body = body
                };
                await smtp.SendMailAsync(mail);

                Debug.WriteLine($"Validation email sent to {user.Email} for user {user.Email} with token {user.EmailValidationToken}");
                return Result.Success();
            }
            catch (SmtpException smtpEx)
            {
                return Result.Failure($"SMTP error while sending validation email: {smtpEx.Message}");
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                return Result.Failure($"HTTP error while sending validation email: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unexpected error while sending validation email: {ex.Message}");
            }
        }

        public async Task<Result> SendPasswordResetAsync(User user)
        {
            // Check configuration and return error messages if missing
            if (string.IsNullOrWhiteSpace(_smtpServer))
                return Result.Failure("EmailSettings:Host is not configured.");
            if (string.IsNullOrWhiteSpace(_smtpUsername))
                return Result.Failure("EmailSettings:Username is not configured.");
            if (string.IsNullOrWhiteSpace(_smtpPassword))
                return Result.Failure("EmailSettings:Password is not configured.");
            if (string.IsNullOrWhiteSpace(_baseUrl))
                return Result.Failure("App:BaseUrl is not configured.");
            if (_smtpPort == null)
                return Result.Failure("EmailSettings:Port is not configured or is invalid.");
            if (_smtpEnableSsl == null)
                return Result.Failure("EmailSettings:EnableSsl is not configured or is invalid.");

            try
            {
                var resetUrl = $"{_baseUrl}/api/PasswordReset/ResetPassword?token={user.PasswordResetToken}";
                var body = $@"Hello {user.Firstname} {user.Lastname},

You have requested to reset your password for your CanoEh account. To reset your password, please click the link below:

{resetUrl}

This link will expire in 24 hours. If you did not request a password reset, please ignore this email.

Best regards,
The CanoEh Team";

                using var smtp = new SmtpClient(_smtpServer, _smtpPort.Value)
                {
                    EnableSsl = _smtpEnableSsl.Value,
                    Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword)
                };
                var mail = new MailMessage(_smtpUsername, user.Email)
                {
                    From = new MailAddress(_fromEmail ?? "Unknown", _fromName),
                    Subject = "Password Reset Request",
                    Body = body
                };
                await smtp.SendMailAsync(mail);

                Debug.WriteLine($"Password reset email sent to {user.Email} with token {user.PasswordResetToken}");
                return Result.Success();
            }
            catch (SmtpException smtpEx)
            {
                return Result.Failure($"SMTP error while sending password reset email: {smtpEx.Message}");
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                return Result.Failure($"HTTP error while sending password reset email: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unexpected error while sending password reset email: {ex.Message}");
            }
        }

        public async Task<Result> SendRestoreUserEmailAsync(User user)
        {
            // Check configuration and return error messages if missing
            if (string.IsNullOrWhiteSpace(_smtpServer))
                return Result.Failure("EmailSettings:Host is not configured.");
            if (string.IsNullOrWhiteSpace(_smtpUsername))
                return Result.Failure("EmailSettings:Username is not configured.");
            if (string.IsNullOrWhiteSpace(_smtpPassword))
                return Result.Failure("EmailSettings:Password is not configured.");
            if (string.IsNullOrWhiteSpace(_baseUrl))
                return Result.Failure("App:BaseUrl is not configured.");
            if (_smtpPort == null)
                return Result.Failure("EmailSettings:Port is not configured or is invalid.");
            if (_smtpEnableSsl == null)
                return Result.Failure("EmailSettings:EnableSsl is not configured or is invalid.");

            try
            {
                var restoreUrl = $"{_baseUrl}/api/User/RestoreUser?token={user.RestoreUserToken}";
                var body = $@"Hello {user.Firstname} {user.Lastname},

You have requested to restore your deleted CanoEh account. To restore your account, please click the link below:

{restoreUrl}

This link will expire in 24 hours. If you did not request account restoration, please ignore this email.

Best regards,
The CanoEh Team";

                using var smtp = new SmtpClient(_smtpServer, _smtpPort.Value)
                {
                    EnableSsl = _smtpEnableSsl.Value,
                    Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword)
                };
                var mail = new MailMessage(_smtpUsername, user.Email)
                {
                    From = new MailAddress(_fromEmail ?? "Unknown", _fromName),
                    Subject = "Account Restoration Request",
                    Body = body
                };
                await smtp.SendMailAsync(mail);

                Debug.WriteLine($"Restore account email sent to {user.Email} with token {user.RestoreUserToken}");
                return Result.Success();
            }
            catch (SmtpException smtpEx)
            {
                return Result.Failure($"SMTP error while sending restore account email: {smtpEx.Message}");
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                return Result.Failure($"HTTP error while sending restore account email: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Unexpected error while sending restore account email: {ex.Message}");
            }
        }
    }
}