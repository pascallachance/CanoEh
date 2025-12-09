using System.Runtime.CompilerServices;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;

namespace Domain.Models.Requests
{
    public class CreateUserRequest
    {
        public required string Email { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public string? Phone { get; set; }
        public required string Password { get; set; }
        public string Language { get; set; } = "en"; // Default to English

        public Result Validate()
        {
            if (this == null)
            {
                return Result.Failure("User data is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Firstname))
            {
                return Result.Failure("First name is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Lastname))
            {
                return Result.Failure("Last name is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                return Result.Failure("Password is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (!IsValidEmail(Email))
            {
                return Result.Failure("Email must be a valid email address.", StatusCodes.Status400BadRequest);
            }
            if (Password.Length < 8)
            {
                return Result.Failure("Password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (!string.IsNullOrWhiteSpace(Language) && Language.Length > 10)
            {
                return Result.Failure("Language code must not exceed 10 characters.", StatusCodes.Status400BadRequest);
            }
            var supportedLanguages = new[] { "en", "fr" };
            if (!string.IsNullOrWhiteSpace(Language) && !supportedLanguages.Contains(Language.ToLowerInvariant()))
            {
                return Result.Failure("Language must be 'en' or 'fr'.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}