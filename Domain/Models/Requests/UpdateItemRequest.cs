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
        public Guid CategoryID { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();

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
            
            if (string.IsNullOrWhiteSpace(Name_fr))
            {
                return Result.Failure("French name is required.", StatusCodes.Status400BadRequest);
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

            if (CategoryID == Guid.Empty)
            {
                return Result.Failure("CategoryID is required.", StatusCodes.Status400BadRequest);
            }
            
            return Result.Success();
        }
    }
}