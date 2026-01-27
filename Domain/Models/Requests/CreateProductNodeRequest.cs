using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateProductNodeRequest
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public required string NodeType { get; set; } // "Departement", "Navigation", or "Category"
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }

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

            if (Name_en.Length > 200)
            {
                return Result.Failure("English name cannot exceed 200 characters.", StatusCodes.Status400BadRequest);
            }

            if (Name_fr.Length > 200)
            {
                return Result.Failure("French name cannot exceed 200 characters.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(NodeType))
            {
                return Result.Failure("NodeType is required.", StatusCodes.Status400BadRequest);
            }

            if (NodeType != "Departement" && NodeType != "Navigation" && NodeType != "Category")
            {
                return Result.Failure("NodeType must be 'Departement', 'Navigation', or 'Category'.", StatusCodes.Status400BadRequest);
            }

            // Departement nodes should not have a parent
            if (NodeType == "Departement" && ParentId.HasValue)
            {
                return Result.Failure("Departement nodes cannot have a parent.", StatusCodes.Status400BadRequest);
            }

            // Navigation and Category nodes must have a parent
            if ((NodeType == "Navigation" || NodeType == "Category") && !ParentId.HasValue)
            {
                return Result.Failure($"{NodeType} nodes must have a parent.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}
