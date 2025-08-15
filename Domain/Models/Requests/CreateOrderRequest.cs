using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class CreateOrderRequest
    {
        public required List<CreateOrderItemRequest> OrderItems { get; set; } = new();
        public required CreateOrderAddressRequest ShippingAddress { get; set; }
        public required CreateOrderAddressRequest BillingAddress { get; set; }
        public required CreateOrderPaymentRequest Payment { get; set; }
        public string? Notes { get; set; }

        public Result Validate()
        {
            if (OrderItems == null || !OrderItems.Any())
            {
                return Result.Failure("At least one order item is required.", StatusCodes.Status400BadRequest);
            }

            if (ShippingAddress == null)
            {
                return Result.Failure("Shipping address is required.", StatusCodes.Status400BadRequest);
            }

            if (BillingAddress == null)
            {
                return Result.Failure("Billing address is required.", StatusCodes.Status400BadRequest);
            }

            if (Payment == null)
            {
                return Result.Failure("Payment information is required.", StatusCodes.Status400BadRequest);
            }

            // Validate each order item
            foreach (var orderItem in OrderItems)
            {
                var itemValidation = orderItem.Validate();
                if (itemValidation.IsFailure)
                {
                    return itemValidation;
                }
            }

            // Validate addresses
            var shippingValidation = ShippingAddress.Validate();
            if (shippingValidation.IsFailure)
            {
                return shippingValidation;
            }

            var billingValidation = BillingAddress.Validate();
            if (billingValidation.IsFailure)
            {
                return billingValidation;
            }

            // Validate payment
            var paymentValidation = Payment.Validate();
            if (paymentValidation.IsFailure)
            {
                return paymentValidation;
            }

            return Result.Success();
        }
    }

    public class CreateOrderItemRequest
    {
        public Guid ItemID { get; set; }
        public Guid ItemVariantID { get; set; }
        public int Quantity { get; set; }

        public Result Validate()
        {
            if (ItemID == Guid.Empty)
            {
                return Result.Failure("Item ID is required.", StatusCodes.Status400BadRequest);
            }

            if (ItemVariantID == Guid.Empty)
            {
                return Result.Failure("Item variant ID is required.", StatusCodes.Status400BadRequest);
            }

            if (Quantity <= 0)
            {
                return Result.Failure("Quantity must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }

    public class CreateOrderAddressRequest
    {
        public required string FullName { get; set; }
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public required string City { get; set; }
        public required string ProvinceState { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }

        public Result Validate()
        {
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

            if (string.IsNullOrWhiteSpace(ProvinceState))
            {
                return Result.Failure("Province/State is required.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(PostalCode))
            {
                return Result.Failure("Postal code is required.", StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(Country))
            {
                return Result.Failure("Country is required.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }

    public class CreateOrderPaymentRequest
    {
        public Guid? PaymentMethodID { get; set; }
        public required string Provider { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Provider))
            {
                return Result.Failure("Payment provider is required.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}