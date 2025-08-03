using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class ChangePasswordRequest
    {
        public string? Email { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return Result.Failure("Email is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                return Result.Failure("Current password is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                return Result.Failure("New password is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                return Result.Failure("Confirm new password is required.", StatusCodes.Status400BadRequest);
            }
            if (!Email.Contains('@'))
            {
                return Result.Failure("Email must contain '@'.", StatusCodes.Status400BadRequest);
            }
            if (CurrentPassword.Length < 8)
            {
                return Result.Failure("Current password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (NewPassword.Length < 8)
            {
                return Result.Failure("New password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (NewPassword != ConfirmNewPassword)
            {
                return Result.Failure("New password and confirm new password do not match.", StatusCodes.Status400BadRequest);
            }
            if (CurrentPassword == NewPassword)
            {
                return Result.Failure("New password must be different from current password.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }
    }
}