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
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name_en) && string.IsNullOrWhiteSpace(Name_fr))
            {
                return Result.Failure("At least one name (English or French) is required.", StatusCodes.Status400BadRequest);
            }
            
            if (SellerID == Guid.Empty)
            {
                return Result.Failure("SellerID is required.", StatusCodes.Status400BadRequest);
            }
            
            return Result.Success();
        }
    }
}