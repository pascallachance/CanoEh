using System.Diagnostics;
using System.Net.Mail;
using Helpers.Common;
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

        public async Task<Result> SendEmailValidationAsync(string email, string username, string validationToken)
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
                var validationUrl = $"{_baseUrl}/api/EmailValidation/ValidateEmail/{validationToken}";
                var body = $@"Hello {username},

Thank you for registering with BaseApp! To complete your registration, please click the link below to validate your email address:

{validationUrl}

If you did not create this account, please ignore this email.

Best regards,
The CanoEh Team";

                using var smtp = new SmtpClient(_smtpServer, _smtpPort.Value)
                {
                    EnableSsl = _smtpEnableSsl.Value,
                    Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword)
                };
                var mail = new MailMessage(_smtpUsername, email)
                {
                    From = new MailAddress(_fromEmail ?? "Unknown", _fromName),
                    Subject = "Email Validation",
                    Body = body
                };
                await smtp.SendMailAsync(mail);

                Debug.WriteLine($"Validation email sent to {email} for user {username} with token {validationToken}");
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
    }
}
