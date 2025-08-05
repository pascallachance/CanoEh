using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateItemRequest
    {
        public Guid Id { get; set; }
        public Guid SellerID { get; set; }
        public required string Name { get; set; }
        public Guid? DescriptionID { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();

        public Result Validate()
        {
            if (Id == Guid.Empty)
            {
                return Result.Failure("Id is required.", StatusCodes.Status400BadRequest);
            }
            
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Result.Failure("Name is required.", StatusCodes.Status400BadRequest);
            }
            
            if (SellerID == Guid.Empty)
            {
                return Result.Failure("SellerID is required.", StatusCodes.Status400BadRequest);
            }
            
            return Result.Success();
        }
    }
}