using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailValidationAsync(string email, string username, Guid userId)
        {
            try
            {
                var validationUrl = $"{_emailSettings.BaseUrl}/api/User/ValidateEmail/{userId}";
                
                // If SMTP credentials are not configured, fall back to debug logging
                if (string.IsNullOrEmpty(_emailSettings.Username) || string.IsNullOrEmpty(_emailSettings.Password))
                {
                    _logger.LogInformation("SMTP not configured, logging email details instead");
                    Debug.WriteLine($"Email validation sent to: {email}");
                    Debug.WriteLine($"Username: {username}");
                    Debug.WriteLine($"Validation link: {validationUrl}");
                    return true;
                }

                var subject = "Email Validation Required - BaseApp";
                var body = $@"
Hello {username},

Thank you for registering with BaseApp! To complete your registration, please click the link below to validate your email address:

{validationUrl}

If you did not create this account, please ignore this email.

Best regards,
The BaseApp Team
";

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = _emailSettings.EnableSsl;
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
                mailMessage.To.Add(email);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = false;

                await client.SendMailAsync(mailMessage);
                
                _logger.LogInformation("Email validation sent successfully to {Email} for user {Username}", email, username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send validation email to {Email} for user {Username}", email, username);
                return false;
            }
        }
    }
}