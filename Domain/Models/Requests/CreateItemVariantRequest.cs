using System.ComponentModel.DataAnnotations;
using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateItemVariantRequest
    {
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? ProductIdentifierType { get; set; }
        public string? ProductIdentifierValue { get; set; }
        public string? ImageUrls { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? ItemVariantName_en { get; set; }
        public string? ItemVariantName_fr { get; set; }
        public List<CreateItemVariantAttributeRequest> ItemVariantAttributes { get; set; } = new();
        public List<CreateItemVariantFeaturesRequest> ItemVariantFeatures { get; set; } = new();
        public bool Deleted { get; set; } = false;
        
        // Offer fields
        [Range(0, 100, ErrorMessage = "Offer must be between 0 and 100")]
        public decimal? Offer { get; set; }
        public DateTime? OfferStart { get; set; }
        public DateTime? OfferEnd { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Sku))
            {
                return Result.Failure("SKU is required for each variant.", StatusCodes.Status400BadRequest);
            }

            if (Sku.Length > 100)
            {
                return Result.Failure("SKU cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
            }

            if (ProductIdentifierType != null && ProductIdentifierType.Length > 50)
            {
                return Result.Failure("Product identifier type cannot exceed 50 characters.", StatusCodes.Status400BadRequest);
            }

            if (ProductIdentifierValue != null && ProductIdentifierValue.Length > 100)
            {
                return Result.Failure("Product identifier value cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
            }

            if (ItemVariantName_en != null && ItemVariantName_en.Length > 255)
            {
                return Result.Failure("Variant name (English) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
            }

            if (ItemVariantName_fr != null && ItemVariantName_fr.Length > 255)
            {
                return Result.Failure("Variant name (French) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
            }

            if (ItemVariantAttributes.Count > ItemValidationLimits.MaxVariantAttributes)
            {
                return Result.Failure($"A variant cannot have more than {ItemValidationLimits.MaxVariantAttributes} attributes.", StatusCodes.Status400BadRequest);
            }

            foreach (var attr in ItemVariantAttributes)
            {
                var attrResult = attr.Validate();
                if (attrResult.IsFailure)
                {
                    return attrResult;
                }
            }

            foreach (var feature in ItemVariantFeatures)
            {
                var featureResult = feature.Validate();
                if (featureResult.IsFailure)
                {
                    return featureResult;
                }
            }

            return Result.Success();
        }
    }
}
