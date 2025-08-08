using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateAddressRequest
    {
        public Guid Id { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public string? State { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public required string AddressType { get; set; } // 'Delivery', 'Billing', 'Company'

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
            if (string.IsNullOrWhiteSpace(Street))
            {
                return Result.Failure("Street is required.", StatusCodes.Status400BadRequest);
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