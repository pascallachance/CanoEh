using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateCategoryMandatoryFeatureDto
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public string? AttributeType { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CreateCategoryNodeRequest
    {
        private const int MaxAttributeNameLength = 100;
        private const int MaxAttributeTypeLength = 50;

        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public required string NodeType { get; set; } // "Departement", "Navigation", or "Category"
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }
        public List<CreateCategoryMandatoryFeatureDto>? CategoryMandatoryFeatures { get; set; }

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

            if (Name_en.Length > BaseNode.MaxNameLength)
            {
                return Result.Failure($"English name cannot exceed {BaseNode.MaxNameLength} characters.", StatusCodes.Status400BadRequest);
            }

            if (Name_fr.Length > BaseNode.MaxNameLength)
            {
                return Result.Failure($"French name cannot exceed {BaseNode.MaxNameLength} characters.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(NodeType))
            {
                return Result.Failure("NodeType is required.", StatusCodes.Status400BadRequest);
            }

            if (NodeType != BaseNode.NodeTypeDepartement && NodeType != BaseNode.NodeTypeNavigation && NodeType != BaseNode.NodeTypeCategory)
            {
                return Result.Failure($"NodeType must be '{BaseNode.NodeTypeDepartement}', '{BaseNode.NodeTypeNavigation}', or '{BaseNode.NodeTypeCategory}'.", StatusCodes.Status400BadRequest);
            }

            // Departement nodes should not have a parent
            if (NodeType == BaseNode.NodeTypeDepartement && ParentId.HasValue)
            {
                return Result.Failure("Departement nodes cannot have a parent.", StatusCodes.Status400BadRequest);
            }

            // Navigation and Category nodes must have a parent
            if ((NodeType == BaseNode.NodeTypeNavigation || NodeType == BaseNode.NodeTypeCategory) && !ParentId.HasValue)
            {
                return Result.Failure($"{NodeType} nodes must have a parent.", StatusCodes.Status400BadRequest);
            }

            // CategoryMandatoryFeatures can only be provided for Category nodes
            if (CategoryMandatoryFeatures != null && CategoryMandatoryFeatures.Any() && NodeType != BaseNode.NodeTypeCategory)
            {
                return Result.Failure("CategoryMandatoryFeatures can only be provided when creating a Category node.", StatusCodes.Status400BadRequest);
            }

            // Validate CategoryMandatoryFeatures if provided
            if (CategoryMandatoryFeatures != null && CategoryMandatoryFeatures.Any())
            {
                foreach (var feature in CategoryMandatoryFeatures)
                {
                    if (string.IsNullOrWhiteSpace(feature.Name_en))
                    {
                        return Result.Failure("CategoryMandatoryFeature English name is required.", StatusCodes.Status400BadRequest);
                    }
                    
                    if (string.IsNullOrWhiteSpace(feature.Name_fr))
                    {
                        return Result.Failure("CategoryMandatoryFeature French name is required.", StatusCodes.Status400BadRequest);
                    }

                    if (feature.Name_en.Length > MaxAttributeNameLength)
                    {
                        return Result.Failure($"CategoryMandatoryFeature English name cannot exceed {MaxAttributeNameLength} characters.", StatusCodes.Status400BadRequest);
                    }

                    if (feature.Name_fr.Length > MaxAttributeNameLength)
                    {
                        return Result.Failure($"CategoryMandatoryFeature French name cannot exceed {MaxAttributeNameLength} characters.", StatusCodes.Status400BadRequest);
                    }

                    if (!string.IsNullOrWhiteSpace(feature.AttributeType) && feature.AttributeType.Length > MaxAttributeTypeLength)
                    {
                        return Result.Failure($"CategoryMandatoryFeature AttributeType cannot exceed {MaxAttributeTypeLength} characters.", StatusCodes.Status400BadRequest);
                    }
                }
            }

            return Result.Success();
        }
    }
}
