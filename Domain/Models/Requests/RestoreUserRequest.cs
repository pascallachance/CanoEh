using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class RestoreUserRequest
    {
        public string? Token { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                return Result.Failure("Restore token is required.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }
    }
}