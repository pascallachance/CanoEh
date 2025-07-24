using System.Runtime.CompilerServices;
using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models
{
    public class CreateUserRequest
    {
        public required string Uname { get; set; }
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public required string Email { get; set; }
        public string? Phone { get; set; }
        public required string Password { get; set; }

        public Result Validate()
        {
            if (this == null)
            {
                return Result.Failure("User data is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Uname))
            {
                return Result.Failure("Username is required.", StatusCodes.Status400BadRequest);
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
            if (Uname.Length < 8)
            {
                return Result.Failure("Username must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (Password.Length < 8)
            {
                return Result.Failure("Password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (!Email.Contains('@'))
            {
                return Result.Failure("Email must contain '@'.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }
    }
}