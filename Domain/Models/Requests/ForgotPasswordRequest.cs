using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class ForgotPasswordRequest
    {
        public string? Email { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (!IsValidEmail(Email))
            {
                return Result.Failure("Invalid email format.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}