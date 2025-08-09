using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateItemRequest
    {
        public Guid SellerID { get; set; }
        public required string Name_en { get; set; }
        public string? Name_fr { get; set; }
        public string? Description_en { get; set; }
        public string? Description_fr { get; set; }
        public Guid CategoryID { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<ItemAttribute> ItemAttributes { get; set; } = new();

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name_en))
            {
                return Result.Failure("English name is required.", StatusCodes.Status400BadRequest);
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