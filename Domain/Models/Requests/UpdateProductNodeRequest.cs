using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateProductNodeRequest
    {
        public required Guid Id { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }

        public Result Validate()
        {
            if (Id == Guid.Empty)
            {
                return Result.Failure("Product node ID cannot be empty.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(Name_en))
            {
                return Result.Failure("English name is required.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(Name_fr))
            {
                return Result.Failure("French name is required.", StatusCodes.Status400BadRequest);
            }

            if (Name_en.Length > BaseNode.MaxNameLength)
            {
                return Result.Failure($"English name cannot exceed {BaseNode.MaxNameLength} characters.", StatusCodes.Status400BadRequest);
            }

            if (Name_fr.Length > BaseNode.MaxNameLength)
            {
                return Result.Failure($"French name cannot exceed {BaseNode.MaxNameLength} characters.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}
