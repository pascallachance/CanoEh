using System.Runtime.CompilerServices;
using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models
{
    public class CreateUser
    {
        public required string uname { get; set; }
        public required string firstname { get; set; }
        public required string lastname { get; set; }
        public required string email { get; set; }
        public string? phone { get; set; }
        public required string password { get; set; }

        public Result Validate()
        {
            if (this == null)
            {
                return Result.Failure("User data is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(uname))
            {
                return Result.Failure("Username is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(firstname))
            {
                return Result.Failure("First name is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(lastname))
            {
                return Result.Failure("Last name is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return Result.Failure("Password is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (uname.Length < 8)
            {
                return Result.Failure("Username must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (password.Length < 8)
            {
                return Result.Failure("Password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (!email.Contains("@"))
            {
                return Result.Failure("Email must contain '@'.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }
    }
}