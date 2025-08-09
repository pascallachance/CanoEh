using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateTaxRateRequest
    {
        public Guid ID { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public required string Country { get; set; }
        public string? ProvinceState { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }

        public Result Validate()
        {
            if (ID == Guid.Empty)
            {
                return Result.Failure("ID is required.", StatusCodes.Status400BadRequest);
            }
            
            if (string.IsNullOrWhiteSpace(Name_en))
            {
                return Result.Failure("English name is required.", StatusCodes.Status400BadRequest);
            }
            
            if (string.IsNullOrWhiteSpace(Name_fr))
            {
                return Result.Failure("French name is required.", StatusCodes.Status400BadRequest);
            }
            
            if (string.IsNullOrWhiteSpace(Country))
            {
                return Result.Failure("Country is required.", StatusCodes.Status400BadRequest);
            }
            
            if (Rate < 0 || Rate > 1)
            {
                return Result.Failure("Rate must be between 0 and 1 (0% to 100%).", StatusCodes.Status400BadRequest);
            }
            
            return Result.Success();
        }
    }
}