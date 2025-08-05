using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateCategoryRequest
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public Result Validate()
        {
            if (Id == Guid.Empty)
            {
                return Result.Failure("Id is required.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                return Result.Failure("Name is required.", StatusCodes.Status400BadRequest);
            }

            if (Name.Length > 100)
            {
                return Result.Failure("Name cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
            }

            if (ParentCategoryId == Id)
            {
                return Result.Failure("Category cannot be its own parent.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}