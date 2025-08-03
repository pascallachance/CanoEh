using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                return Result.Failure("Password is required.", StatusCodes.Status400BadRequest);
            }
            if (!IsValidEmail(Email))
            {
                return Result.Failure("Email format is invalid.", StatusCodes.Status400BadRequest);
            }
            if (Password.Length < 8)
            {
                return Result.Failure("Password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
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
