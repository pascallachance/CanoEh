using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateCategoryRequest
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public Guid? ParentCategoryId { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name_en))
            {
                return Result.Failure("English name is required.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(Name_fr))
            {
                return Result.Failure("French name is required.", StatusCodes.Status400BadRequest);
            }

            if (Name_en.Length > 100)
            {
                return Result.Failure("English name cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
            }

            if (Name_fr.Length > 100)
            {
                return Result.Failure("French name cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}