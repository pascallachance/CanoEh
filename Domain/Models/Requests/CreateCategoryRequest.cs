using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateCategoryRequest
    {
        public required string Name { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Result.Failure("Name is required.", StatusCodes.Status400BadRequest);
            }

            if (Name.Length > 100)
            {
                return Result.Failure("Name cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}