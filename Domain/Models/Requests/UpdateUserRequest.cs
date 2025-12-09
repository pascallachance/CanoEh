using System.Runtime.CompilerServices;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;

namespace Domain.Models.Requests
{
    public class UpdateUserRequest
    {
        public required string Email { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public string? Phone { get; set; }
        public string? Language { get; set; } // Optional - only update if provided

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
            if (string.IsNullOrWhiteSpace(Email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (!IsValidEmail(Email))
            {
                return Result.Failure("Email must be a valid email address.", StatusCodes.Status400BadRequest);
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