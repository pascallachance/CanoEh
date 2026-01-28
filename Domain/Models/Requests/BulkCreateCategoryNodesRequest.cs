using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CategoryNodeDto
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }
        public List<CreateCategoryMandatoryAttributeDto>? CategoryMandatoryAttributes { get; set; }
    }

    public class NavigationNodeDto
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }
        public List<NavigationNodeDto>? NavigationNodes { get; set; }
        public List<CategoryNodeDto>? CategoryNodes { get; set; }
    }

    public class DepartementNodeDto
    {
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }
        public List<NavigationNodeDto>? NavigationNodes { get; set; }
        public List<CategoryNodeDto>? CategoryNodes { get; set; }
    }

    public class BulkCreateCategoryNodesRequest
    {
        public List<DepartementNodeDto> Departements { get; set; } = new();

        public Result Validate()
        {
            if (Departements == null || !Departements.Any())
            {
                return Result.Failure("At least one Departement node is required.", StatusCodes.Status400BadRequest);
            }

            // Validate all departments and return first failure if any
            var failedValidation = Departements
                .Select(ValidateDepartementNode)
                .FirstOrDefault(v => v.IsFailure);

            return failedValidation ?? Result.Success();
        }

        private static Result ValidateDepartementNode(DepartementNodeDto dept)
        {
            var basicValidation = ValidateBasicNodeProperties(dept.Name_en, dept.Name_fr, "Departement");
            if (basicValidation.IsFailure)
            {
                return basicValidation;
            }

            if (dept.NavigationNodes != null)
            {
                var navValidation = dept.NavigationNodes
                    .Select(ValidateNavigationNode)
                    .FirstOrDefault(v => v.IsFailure);
                
                if (navValidation != null)
                {
                    return navValidation;
                }
            }

            if (dept.CategoryNodes != null)
            {
                var catValidation = dept.CategoryNodes
                    .Select(ValidateCategoryNode)
                    .FirstOrDefault(v => v.IsFailure);
                
                if (catValidation != null)
                {
                    return catValidation;
                }
            }

            return Result.Success();
        }

        private static Result ValidateNavigationNode(NavigationNodeDto nav)
        {
            var basicValidation = ValidateBasicNodeProperties(nav.Name_en, nav.Name_fr, "Navigation");
            if (basicValidation.IsFailure)
            {
                return basicValidation;
            }

            if (nav.NavigationNodes != null)
            {
                var navValidation = nav.NavigationNodes
                    .Select(ValidateNavigationNode)
                    .FirstOrDefault(v => v.IsFailure);
                
                if (navValidation != null)
                {
                    return navValidation;
                }
            }

            if (nav.CategoryNodes != null)
            {
                var catValidation = nav.CategoryNodes
                    .Select(ValidateCategoryNode)
                    .FirstOrDefault(v => v.IsFailure);
                
                if (catValidation != null)
                {
                    return catValidation;
                }
            }

            return Result.Success();
        }

        private static Result ValidateCategoryNode(CategoryNodeDto cat)
        {
            var basicValidation = ValidateBasicNodeProperties(cat.Name_en, cat.Name_fr, "Category");
            if (basicValidation.IsFailure)
            {
                return basicValidation;
            }

            if (cat.CategoryMandatoryAttributes != null && cat.CategoryMandatoryAttributes.Any())
            {
                foreach (var attr in cat.CategoryMandatoryAttributes)
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

            return Result.Success();
        }

        private static Result ValidateBasicNodeProperties(string nameEn, string nameFr, string nodeType)
        {
            if (string.IsNullOrWhiteSpace(nameEn))
            {
                return Result.Failure($"{nodeType} node English name is required.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(nameFr))
            {
                return Result.Failure($"{nodeType} node French name is required.", StatusCodes.Status400BadRequest);
            }

            if (nameEn.Length > BaseNode.MaxNameLength)
            {
                return Result.Failure($"{nodeType} node English name cannot exceed {BaseNode.MaxNameLength} characters.", StatusCodes.Status400BadRequest);
            }

            if (nameFr.Length > BaseNode.MaxNameLength)
            {
                return Result.Failure($"{nodeType} node French name cannot exceed {BaseNode.MaxNameLength} characters.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}
