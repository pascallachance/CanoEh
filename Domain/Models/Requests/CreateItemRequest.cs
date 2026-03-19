using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateItemRequest
    {
        public Guid SellerID { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public required string Description_en { get; set; }
        public required string Description_fr { get; set; }
        public Guid CategoryNodeID { get; set; }
        public List<CreateItemVariantRequest> Variants { get; set; } = new();
        public List<CreateItemVariantFeaturesRequest> ItemVariantFeatures { get; set; } = new();

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name_en))
            {
                return Result.Failure("English name is required.", StatusCodes.Status400BadRequest);
            }

            if (Name_en.Length > 255)
            {
                return Result.Failure("English name cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(Name_fr))
            {
                return Result.Failure("French name is required.", StatusCodes.Status400BadRequest);
            }

            if (Name_fr.Length > 255)
            {
                return Result.Failure("French name cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(Description_en))
            {
                return Result.Failure("English description is required.", StatusCodes.Status400BadRequest);
            }
            
            if (string.IsNullOrWhiteSpace(Description_fr))
            {
                return Result.Failure("French description is required.", StatusCodes.Status400BadRequest);
            }
            
            if (SellerID == Guid.Empty)
            {
                return Result.Failure("SellerID is required.", StatusCodes.Status400BadRequest);
            }

            if (CategoryNodeID == Guid.Empty)
            {
                return Result.Failure("CategoryNodeID is required.", StatusCodes.Status400BadRequest);
            }

            foreach (var variant in Variants)
            {
                var variantResult = variant.Validate();
                if (variantResult.IsFailure)
                {
                    return variantResult;
                }
            }

            return Result.Success();
        }
    }
}