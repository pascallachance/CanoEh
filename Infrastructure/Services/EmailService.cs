using System.Diagnostics;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        public async Task<bool> SendEmailValidationAsync(string email, string username, Guid userId)
        {
            // TODO: Implement actual email sending logic
            // For now, this is a stub implementation that logs the email details
            await Task.Delay(100); // Simulate async operation
            
            Debug.WriteLine($"Email validation sent to: {email}");
            Debug.WriteLine($"Username: {username}");
            Debug.WriteLine($"Validation link: https://localhost:7182/api/User/ValidateEmail/{userId}");
            
            return true;
        }
    }
}