using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class ResetPasswordRequest
    {
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                return Result.Failure("Reset token is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                return Result.Failure("New password is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(ConfirmNewPassword))
            {
                return Result.Failure("Confirm new password is required.", StatusCodes.Status400BadRequest);
            }
            if (NewPassword.Length < 8)
            {
                return Result.Failure("New password must be at least 8 characters long.", StatusCodes.Status400BadRequest);
            }
            if (NewPassword != ConfirmNewPassword)
            {
                return Result.Failure("New password and confirm new password do not match.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }
    }
}