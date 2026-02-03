using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateCategoryMandatoryAttributeDto
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public string? AttributeType { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CreateCategoryMandatoryExtraAttributeDto
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public string? AttributeType { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CreateCategoryNodeRequest
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public required string NodeType { get; set; } // "Departement", "Navigation", or "Category"
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }
        public List<CreateCategoryMandatoryAttributeDto>? CategoryMandatoryAttributes { get; set; }
        public List<CreateCategoryMandatoryExtraAttributeDto>? CategoryMandatoryExtraAttributes { get; set; }

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

            // CategoryMandatoryAttributes can only be provided for Category nodes
            if (CategoryMandatoryAttributes != null && CategoryMandatoryAttributes.Any() && NodeType != BaseNode.NodeTypeCategory)
            {
                return Result.Failure("CategoryMandatoryAttributes can only be provided when creating a Category node.", StatusCodes.Status400BadRequest);
            }

            // CategoryMandatoryExtraAttributes can only be provided for Category nodes
            if (CategoryMandatoryExtraAttributes != null && CategoryMandatoryExtraAttributes.Any() && NodeType != BaseNode.NodeTypeCategory)
            {
                return Result.Failure("CategoryMandatoryExtraAttributes can only be provided when creating a Category node.", StatusCodes.Status400BadRequest);
            }

            // Validate CategoryMandatoryAttributes if provided
            if (CategoryMandatoryAttributes != null && CategoryMandatoryAttributes.Any())
            {
                foreach (var attr in CategoryMandatoryAttributes)
                {
                    if (string.IsNullOrWhiteSpace(attr.Name_en))
                    {
                        return Result.Failure("CategoryMandatoryAttribute English name is required.", StatusCodes.Status400BadRequest);
                    }
                    
                    if (string.IsNullOrWhiteSpace(attr.Name_fr))
                    {
                        return Result.Failure("CategoryMandatoryAttribute French name is required.", StatusCodes.Status400BadRequest);
                    }

                    const int MaxAttributeNameLength = 100;
                    if (attr.Name_en.Length > MaxAttributeNameLength)
                    {
                        return Result.Failure($"CategoryMandatoryAttribute English name cannot exceed {MaxAttributeNameLength} characters.", StatusCodes.Status400BadRequest);
                    }

                    if (attr.Name_fr.Length > MaxAttributeNameLength)
                    {
                        return Result.Failure($"CategoryMandatoryAttribute French name cannot exceed {MaxAttributeNameLength} characters.", StatusCodes.Status400BadRequest);
                    }

                    const int MaxAttributeTypeLength = 50;
                    if (!string.IsNullOrWhiteSpace(attr.AttributeType) && attr.AttributeType.Length > MaxAttributeTypeLength)
                    {
                        return Result.Failure($"CategoryMandatoryAttribute AttributeType cannot exceed {MaxAttributeTypeLength} characters.", StatusCodes.Status400BadRequest);
                    }
                }
            }

            // Validate CategoryMandatoryExtraAttributes if provided
            if (CategoryMandatoryExtraAttributes != null && CategoryMandatoryExtraAttributes.Any())
            {
                foreach (var attr in CategoryMandatoryExtraAttributes)
                {
                    if (string.IsNullOrWhiteSpace(attr.Name_en))
                    {
                        return Result.Failure("CategoryMandatoryExtraAttribute English name is required.", StatusCodes.Status400BadRequest);
                    }
                    
                    if (string.IsNullOrWhiteSpace(attr.Name_fr))
                    {
                        return Result.Failure("CategoryMandatoryExtraAttribute French name is required.", StatusCodes.Status400BadRequest);
                    }

                    const int MaxAttributeNameLength = 100;
                    if (attr.Name_en.Length > MaxAttributeNameLength)
                    {
                        return Result.Failure($"CategoryMandatoryExtraAttribute English name cannot exceed {MaxAttributeNameLength} characters.", StatusCodes.Status400BadRequest);
                    }

                    if (attr.Name_fr.Length > MaxAttributeNameLength)
                    {
                        return Result.Failure($"CategoryMandatoryExtraAttribute French name cannot exceed {MaxAttributeNameLength} characters.", StatusCodes.Status400BadRequest);
                    }

                    const int MaxAttributeTypeLength = 50;
                    if (!string.IsNullOrWhiteSpace(attr.AttributeType) && attr.AttributeType.Length > MaxAttributeTypeLength)
                    {
                        return Result.Failure($"CategoryMandatoryExtraAttribute AttributeType cannot exceed {MaxAttributeTypeLength} characters.", StatusCodes.Status400BadRequest);
                    }
                }
            }

            return Result.Success();
        }
    }
}
