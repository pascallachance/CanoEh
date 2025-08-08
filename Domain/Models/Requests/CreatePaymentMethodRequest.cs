using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreatePaymentMethodRequest
    {
        public required string Type { get; set; } // 'Credit Card', 'Debit Card', 'PayPal', etc.
        public string? CardHolderName { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; } // 'Visa', 'MasterCard', 'Amex', etc.
        public int? ExpirationMonth { get; set; }
        public int? ExpirationYear { get; set; }
        public string? BillingAddress { get; set; }
        public bool IsDefault { get; set; }

        public Result Validate()
        {
            if (this == null)
            {
                return Result.Failure("Payment method data is required.", StatusCodes.Status400BadRequest);
            }
            if (string.IsNullOrWhiteSpace(Type))
            {
                return Result.Failure("Payment method type is required.", StatusCodes.Status400BadRequest);
            }
            if (!IsValidPaymentMethodType(Type))
            {
                return Result.Failure("Payment method type must be 'Credit Card', 'Debit Card', 'PayPal', or 'Bank Transfer'.", StatusCodes.Status400BadRequest);
            }

            // For card types, validate card-specific fields
            if (IsCardType(Type))
            {
                if (string.IsNullOrWhiteSpace(CardHolderName))
                {
                    return Result.Failure("Card holder name is required for card payments.", StatusCodes.Status400BadRequest);
                }
                if (string.IsNullOrWhiteSpace(CardLast4) || CardLast4.Length != 4)
                {
                    return Result.Failure("Card last 4 digits are required and must be exactly 4 digits.", StatusCodes.Status400BadRequest);
                }
                if (!ExpirationMonth.HasValue || ExpirationMonth < 1 || ExpirationMonth > 12)
                {
                    return Result.Failure("Valid expiration month (1-12) is required for card payments.", StatusCodes.Status400BadRequest);
                }
                if (!ExpirationYear.HasValue || ExpirationYear < DateTime.Now.Year)
                {
                    return Result.Failure("Valid expiration year is required for card payments.", StatusCodes.Status400BadRequest);
                }
                if (string.IsNullOrWhiteSpace(CardBrand))
                {
                    return Result.Failure("Card brand is required for card payments.", StatusCodes.Status400BadRequest);
                }
            }

            return Result.Success();
        }

        private static bool IsValidPaymentMethodType(string type)
        {
            return type.Equals("Credit Card", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Debit Card", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("PayPal", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Bank Transfer", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCardType(string type)
        {
            return type.Equals("Credit Card", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Debit Card", StringComparison.OrdinalIgnoreCase);
        }
    }
}