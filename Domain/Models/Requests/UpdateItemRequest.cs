using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateItemRequest
    {
        public Guid Id { get; set; }
        public Guid SellerID { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public required string Description_en { get; set; }
        public required string Description_fr { get; set; }
        public Guid CategoryNodeID { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<ItemVariantFeatures> ItemVariantFeatures { get; set; } = new();

        public Result Validate()
        {
            if (Id == Guid.Empty)
            {
                return Result.Failure("Id is required.", StatusCodes.Status400BadRequest);
            }
            
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
                if (string.IsNullOrWhiteSpace(variant.Sku))
                {
                    return Result.Failure("SKU is required for each variant.", StatusCodes.Status400BadRequest);
                }

                if (variant.Sku.Length > 100)
                {
                    return Result.Failure("SKU cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
                }

                if (variant.ProductIdentifierType != null && variant.ProductIdentifierType.Length > 50)
                {
                    return Result.Failure("Product identifier type cannot exceed 50 characters.", StatusCodes.Status400BadRequest);
                }

                if (variant.ProductIdentifierValue != null && variant.ProductIdentifierValue.Length > 100)
                {
                    return Result.Failure("Product identifier value cannot exceed 100 characters.", StatusCodes.Status400BadRequest);
                }

                if (variant.ItemVariantName_en != null && variant.ItemVariantName_en.Length > 255)
                {
                    return Result.Failure("Variant name (English) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
                }

                if (variant.ItemVariantName_fr != null && variant.ItemVariantName_fr.Length > 255)
                {
                    return Result.Failure("Variant name (French) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
                }

                foreach (var attr in variant.ItemVariantAttributes)
                {
                    if (attr.AttributeName_en != null && attr.AttributeName_en.Length > 255)
                    {
                        return Result.Failure("Attribute name (English) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
                    }

                    if (attr.AttributeName_fr != null && attr.AttributeName_fr.Length > 255)
                    {
                        return Result.Failure("Attribute name (French) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
                    }
                }

                foreach (var feature in variant.ItemVariantFeatures)
                {
                    if (feature.AttributeName_en != null && feature.AttributeName_en.Length > 255)
                    {
                        return Result.Failure("Feature name (English) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
                    }

                    if (feature.AttributeName_fr != null && feature.AttributeName_fr.Length > 255)
                    {
                        return Result.Failure("Feature name (French) cannot exceed 255 characters.", StatusCodes.Status400BadRequest);
                    }
                }
            }

            return Result.Success();
        }
    }
}