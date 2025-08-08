using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateAddressRequest
    {
        public Guid Id { get; set; }
        public required string FullName { get; set; }
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public required string City { get; set; }
        public string? ProvinceState { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public required string AddressType { get; set; } // 'Delivery', 'Billing', 'Company'
        public bool IsDefault { get; set; }

        public Result Validate()
        {
            if (this == null)
            {
                return Result.Failure("Address data is required.", StatusCodes.Status400BadRequest);
            }
            if (Id == Guid.Empty)
            {
                return Result.Failure("Address ID is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(FullName))
            {
                return Result.Failure("Full name is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(AddressLine1))
            {
                return Result.Failure("Address line 1 is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(City))
            {
                return Result.Failure("City is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(PostalCode))
            {
                return Result.Failure("Postal code is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Country))
            {
                return Result.Failure("Country is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(AddressType))
            {
                return Result.Failure("Address type is required.", StatusCodes.Status400BadRequest);
            }
            if (!IsValidAddressType(AddressType))
            {
                return Result.Failure("Address type must be 'Delivery', 'Billing', or 'Company'.", StatusCodes.Status400BadRequest);
            }
            return Result.Success();
        }

        private static bool IsValidAddressType(string addressType)
        {
            return addressType.Equals("Delivery", StringComparison.OrdinalIgnoreCase) ||
                   addressType.Equals("Billing", StringComparison.OrdinalIgnoreCase) ||
                   addressType.Equals("Company", StringComparison.OrdinalIgnoreCase);
        }
    }
}