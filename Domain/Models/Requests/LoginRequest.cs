using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                return Result.Failure("Username is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                return Result.Failure("Password is required.", StatusCodes.Status400BadRequest);
            }
            if (Username.Length < 8)
            {
                return Result.Failure("Username must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (Password.Length < 8)
            {
                return Result.Failure("Password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }
    }
}
