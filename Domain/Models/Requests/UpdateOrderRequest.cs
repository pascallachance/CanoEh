using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Domain.Models.Requests
{
    public class UpdateOrderRequest
    {
        public Guid ID { get; set; }
        public string? StatusCode { get; set; }
        public string? Notes { get; set; }
        public List<UpdateOrderItemRequest>? OrderItems { get; set; }

        public Result Validate()
        {
            if (ID == Guid.Empty)
            {
                return Result.Failure("Order ID is required.", StatusCodes.Status400BadRequest);
            }

            // Validate order items if provided
            if (OrderItems != null)
            {
                foreach (var orderItem in OrderItems)
                {
                    var itemValidation = orderItem.Validate();
                    if (itemValidation.IsFailure)
                    {
                        return itemValidation;
                    }
                }
            }

            return Result.Success();
        }
    }

    public class UpdateOrderItemRequest
    {
        public Guid ID { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set; }
        public string? OnHoldReason { get; set; }

        public Result Validate()
        {
            if (ID == Guid.Empty)
            {
                return Result.Failure("Order item ID is required.", StatusCodes.Status400BadRequest);
            }

            if (Quantity.HasValue && Quantity <= 0)
            {
                return Result.Failure("Quantity must be greater than zero.", StatusCodes.Status400BadRequest);
            }

            return Result.Success();
        }
    }
}