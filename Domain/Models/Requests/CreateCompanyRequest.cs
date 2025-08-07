using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateCompanyRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }

        public Result Validate()
        {
            if (this == null)
            {
                return Result.Failure("Company data is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Result.Failure("Name is required.", StatusCodes.Status400BadRequest);
            }
            if (Name.Length > 255)
            {
                return Result.Failure("Name must be 255 characters or less.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }
    }
}