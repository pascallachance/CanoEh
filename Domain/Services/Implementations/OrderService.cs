using System.Diagnostics;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderAddressRepository _orderAddressRepository;
        private readonly IOrderPaymentRepository _orderPaymentRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IUserRepository _userRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IOrderAddressRepository orderAddressRepository,
            IOrderPaymentRepository orderPaymentRepository,
            IOrderStatusRepository orderStatusRepository,
            IItemRepository itemRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderAddressRepository = orderAddressRepository;
            _orderPaymentRepository = orderPaymentRepository;
            _orderStatusRepository = orderStatusRepository;
            _itemRepository = itemRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<CreateOrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest createRequest)
        {
            try
            {
                var validationResult = createRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateOrderResponse>(
                        validationResult.Error ?? "Validation failed.",
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                // Verify user exists
                var userExists = await _userRepository.ExistsAsync(userId);
                if (!userExists)
                {
                    return Result.Failure<CreateOrderResponse>("User not found.", StatusCodes.Status404NotFound);
                }

                // Get pending status
                var pendingStatus = await _orderStatusRepository.FindByStatusCodeAsync("Pending");
                if (pendingStatus == null)
                {
                    return Result.Failure<CreateOrderResponse>("Order status 'Pending' not found.", StatusCodes.Status500InternalServerError);
                }

                var orderId = Guid.NewGuid();
                decimal subtotal = 0;
                var orderItems = new List<OrderItem>();

                // Validate items and calculate totals
                foreach (var orderItemRequest in createRequest.OrderItems)
                {
                    var item = await _itemRepository.GetByIdAsync(orderItemRequest.ItemID);
                    if (item == null || item.Deleted)
                    {
                        return Result.Failure<CreateOrderResponse>($"Item with ID {orderItemRequest.ItemID} not found.", StatusCodes.Status404NotFound);
                    }

                    var variant = item.Variants.FirstOrDefault(v => v.Id == orderItemRequest.ItemVariantID && !v.Deleted);
                    if (variant == null)
                    {
                        return Result.Failure<CreateOrderResponse>($"Item variant with ID {orderItemRequest.ItemVariantID} not found.", StatusCodes.Status404NotFound);
                    }

                    // Check stock availability
                    if (variant.StockQuantity < orderItemRequest.Quantity)
                    {
                        return Result.Failure<CreateOrderResponse>($"Insufficient stock for item '{item.Name_en}'. Available: {variant.StockQuantity}, Requested: {orderItemRequest.Quantity}", StatusCodes.Status400BadRequest);
                    }

                    var orderItem = new OrderItem
                    {
                        ID = Guid.NewGuid(),
                        OrderID = orderId,
                        ItemID = orderItemRequest.ItemID,
                        ItemVariantID = orderItemRequest.ItemVariantID,
                        Name_en = item.Name_en,
                        Name_fr = item.Name_fr,
                        VariantName_en = variant.ItemVariantName_en,
                        VariantName_fr = variant.ItemVariantName_fr,
                        Quantity = orderItemRequest.Quantity,
                        UnitPrice = variant.Price,
                        TotalPrice = variant.Price * orderItemRequest.Quantity,
                        Status = "Pending"
                    };

                    orderItems.Add(orderItem);
                    subtotal += orderItem.TotalPrice;
                }

                // Calculate tax and shipping (simplified - should be configurable)
                decimal taxRate = 0.13m; // Example tax rate
                decimal shippingRate = 10.00m; // Example shipping rate
                decimal taxTotal = subtotal * taxRate;
                decimal shippingTotal = subtotal > 50 ? 0 : shippingRate; // Free shipping over $50
                decimal grandTotal = subtotal + taxTotal + shippingTotal;

                // Create order
                var order = new Order
                {
                    ID = orderId,
                    UserID = userId,
                    OrderDate = DateTime.UtcNow,
                    StatusID = pendingStatus.ID,
                    Subtotal = subtotal,
                    TaxTotal = taxTotal,
                    ShippingTotal = shippingTotal,
                    GrandTotal = grandTotal,
                    Notes = createRequest.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                // Save order
                var createdOrder = await _orderRepository.AddAsync(order);

                // Save order items
                foreach (var orderItem in orderItems)
                {
                    await _orderItemRepository.AddAsync(orderItem);
                }

                // Save addresses
                var shippingAddress = new OrderAddress
                {
                    ID = Guid.NewGuid(),
                    OrderID = orderId,
                    Type = "Shipping",
                    FullName = createRequest.ShippingAddress.FullName,
                    AddressLine1 = createRequest.ShippingAddress.AddressLine1,
                    AddressLine2 = createRequest.ShippingAddress.AddressLine2,
                    AddressLine3 = createRequest.ShippingAddress.AddressLine3,
                    City = createRequest.ShippingAddress.City,
                    ProvinceState = createRequest.ShippingAddress.ProvinceState,
                    PostalCode = createRequest.ShippingAddress.PostalCode,
                    Country = createRequest.ShippingAddress.Country
                };

                var billingAddress = new OrderAddress
                {
                    ID = Guid.NewGuid(),
                    OrderID = orderId,
                    Type = "Billing",
                    FullName = createRequest.BillingAddress.FullName,
                    AddressLine1 = createRequest.BillingAddress.AddressLine1,
                    AddressLine2 = createRequest.BillingAddress.AddressLine2,
                    AddressLine3 = createRequest.BillingAddress.AddressLine3,
                    City = createRequest.BillingAddress.City,
                    ProvinceState = createRequest.BillingAddress.ProvinceState,
                    PostalCode = createRequest.BillingAddress.PostalCode,
                    Country = createRequest.BillingAddress.Country
                };

                await _orderAddressRepository.AddAsync(shippingAddress);
                await _orderAddressRepository.AddAsync(billingAddress);

                // Save payment
                var payment = new OrderPayment
                {
                    ID = Guid.NewGuid(),
                    OrderID = orderId,
                    PaymentMethodID = createRequest.Payment.PaymentMethodID,
                    Amount = grandTotal,
                    Provider = createRequest.Payment.Provider
                };

                await _orderPaymentRepository.AddAsync(payment);

                // TODO: Decrease stock quantities - this should be done in a transaction
                // For now, this is simplified and should be enhanced with proper transaction handling

                return Result.Success(new CreateOrderResponse
                {
                    ID = createdOrder.ID,
                    UserID = createdOrder.UserID,
                    OrderNumber = createdOrder.OrderNumber,
                    OrderDate = createdOrder.OrderDate,
                    StatusCode = pendingStatus.StatusCode,
                    StatusName_en = pendingStatus.Name_en,
                    StatusName_fr = pendingStatus.Name_fr,
                    Subtotal = createdOrder.Subtotal,
                    TaxTotal = createdOrder.TaxTotal,
                    ShippingTotal = createdOrder.ShippingTotal,
                    GrandTotal = createdOrder.GrandTotal,
                    Notes = createdOrder.Notes,
                    CreatedAt = createdOrder.CreatedAt,
                    UpdatedAt = createdOrder.UpdatedAt,
                    OrderItems = orderItems.Select(oi => new OrderItemResponse
                    {
                        ID = oi.ID,
                        OrderID = oi.OrderID,
                        ItemID = oi.ItemID,
                        ItemVariantID = oi.ItemVariantID,
                        Name_en = oi.Name_en,
                        Name_fr = oi.Name_fr,
                        VariantName_en = oi.VariantName_en,
                        VariantName_fr = oi.VariantName_fr,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Status = oi.Status,
                        DeliveredAt = oi.DeliveredAt,
                        OnHoldReason = oi.OnHoldReason
                    }).ToList(),
                    Addresses = new List<OrderAddressResponse>
                    {
                        MapToOrderAddressResponse(shippingAddress),
                        MapToOrderAddressResponse(billingAddress)
                    },
                    Payment = new OrderPaymentResponse
                    {
                        ID = payment.ID,
                        OrderID = payment.OrderID,
                        PaymentMethodID = payment.PaymentMethodID,
                        Amount = payment.Amount,
                        Provider = payment.Provider,
                        ProviderReference = payment.ProviderReference,
                        PaidAt = payment.PaidAt
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating order: {ex.Message}");
                return Result.Failure<CreateOrderResponse>($"An error occurred while creating the order: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetOrderResponse>> GetOrderAsync(Guid userId, Guid orderId)
        {
            try
            {
                var order = await _orderRepository.FindByUserIdAndIdAsync(userId, orderId);
                if (order == null)
                {
                    return Result.Failure<GetOrderResponse>("Order not found.", StatusCodes.Status404NotFound);
                }

                return await BuildGetOrderResponse(order);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting order: {ex.Message}");
                return Result.Failure<GetOrderResponse>($"An error occurred while retrieving the order: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetOrderResponse>> GetOrderByOrderNumberAsync(Guid userId, int orderNumber)
        {
            try
            {
                var order = await _orderRepository.FindByUserIdAndOrderNumberAsync(userId, orderNumber);
                if (order == null)
                {
                    return Result.Failure<GetOrderResponse>("Order not found.", StatusCodes.Status404NotFound);
                }

                return await BuildGetOrderResponse(order);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting order by number: {ex.Message}");
                return Result.Failure<GetOrderResponse>($"An error occurred while retrieving the order: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetOrderResponse>>> GetUserOrdersAsync(Guid userId)
        {
            try
            {
                var orders = await _orderRepository.FindByUserIdAsync(userId);
                var responses = new List<GetOrderResponse>();

                foreach (var order in orders)
                {
                    var responseResult = await BuildGetOrderResponse(order);
                    if (responseResult.IsSuccess)
                    {
                        responses.Add(responseResult.Value!);
                    }
                }

                return Result.Success<IEnumerable<GetOrderResponse>>(responses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user orders: {ex.Message}");
                return Result.Failure<IEnumerable<GetOrderResponse>>($"An error occurred while retrieving orders: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetOrderResponse>>> GetUserOrdersByStatusAsync(Guid userId, string statusCode)
        {
            try
            {
                var orders = await _orderRepository.FindByUserIdAndStatusAsync(userId, statusCode);
                var responses = new List<GetOrderResponse>();

                foreach (var order in orders)
                {
                    var responseResult = await BuildGetOrderResponse(order);
                    if (responseResult.IsSuccess)
                    {
                        responses.Add(responseResult.Value!);
                    }
                }

                return Result.Success<IEnumerable<GetOrderResponse>>(responses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user orders by status: {ex.Message}");
                return Result.Failure<IEnumerable<GetOrderResponse>>($"An error occurred while retrieving orders: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateOrderResponse>> UpdateOrderAsync(Guid userId, UpdateOrderRequest updateRequest)
        {
            try
            {
                var validationResult = updateRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdateOrderResponse>(
                        validationResult.Error ?? "Validation failed.",
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                // Check if user can modify this order
                var canModify = await _orderRepository.CanUserModifyOrderAsync(userId, updateRequest.ID);
                if (!canModify)
                {
                    return Result.Failure<UpdateOrderResponse>("Order cannot be modified. Either it doesn't exist, you don't have permission, or its status doesn't allow modifications.", StatusCodes.Status403Forbidden);
                }

                var order = await _orderRepository.FindByUserIdAndIdAsync(userId, updateRequest.ID);
                if (order == null)
                {
                    return Result.Failure<UpdateOrderResponse>("Order not found.", StatusCodes.Status404NotFound);
                }

                // Update status if provided
                if (!string.IsNullOrWhiteSpace(updateRequest.StatusCode))
                {
                    var status = await _orderStatusRepository.FindByStatusCodeAsync(updateRequest.StatusCode);
                    if (status == null)
                    {
                        return Result.Failure<UpdateOrderResponse>($"Order status '{updateRequest.StatusCode}' not found.", StatusCodes.Status400BadRequest);
                    }
                    order.StatusID = status.ID;
                }

                // Update notes if provided
                if (updateRequest.Notes != null)
                {
                    order.Notes = updateRequest.Notes;
                }

                order.UpdatedAt = DateTime.UtcNow;

                // Update order items if provided
                if (updateRequest.OrderItems != null)
                {
                    foreach (var orderItemUpdate in updateRequest.OrderItems)
                    {
                        var orderItem = await _orderItemRepository.GetByIdAsync(orderItemUpdate.ID);
                        if (orderItem == null || orderItem.OrderID != order.ID)
                        {
                            return Result.Failure<UpdateOrderResponse>($"Order item with ID {orderItemUpdate.ID} not found in this order.", StatusCodes.Status404NotFound);
                        }

                        if (orderItemUpdate.Quantity.HasValue)
                        {
                            // Recalculate total price
                            orderItem.Quantity = orderItemUpdate.Quantity.Value;
                            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
                        }

                        if (!string.IsNullOrWhiteSpace(orderItemUpdate.Status))
                        {
                            orderItem.Status = orderItemUpdate.Status;
                            if (orderItemUpdate.Status == "Delivered")
                            {
                                orderItem.DeliveredAt = DateTime.UtcNow;
                            }
                        }

                        if (orderItemUpdate.OnHoldReason != null)
                        {
                            orderItem.OnHoldReason = orderItemUpdate.OnHoldReason;
                        }

                        await _orderItemRepository.UpdateAsync(orderItem);
                    }

                    // Recalculate order totals if quantities changed
                    var allOrderItems = await _orderItemRepository.FindByOrderIdAsync(order.ID);
                    order.Subtotal = allOrderItems.Sum(oi => oi.TotalPrice);
                    order.TaxTotal = order.Subtotal * 0.13m; // Simplified tax calculation
                    order.GrandTotal = order.Subtotal + order.TaxTotal + order.ShippingTotal;
                }

                await _orderRepository.UpdateAsync(order);

                return await BuildUpdateOrderResponse(order);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating order: {ex.Message}");
                return Result.Failure<UpdateOrderResponse>($"An error occurred while updating the order: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid userId, Guid orderId)
        {
            try
            {
                // Check if user can modify this order
                var canModify = await _orderRepository.CanUserModifyOrderAsync(userId, orderId);
                if (!canModify)
                {
                    return Result.Failure<DeleteOrderResponse>("Order cannot be deleted. Either it doesn't exist, you don't have permission, or its status doesn't allow deletion.", StatusCodes.Status403Forbidden);
                }

                var order = await _orderRepository.FindByUserIdAndIdAsync(userId, orderId);
                if (order == null)
                {
                    return Result.Failure<DeleteOrderResponse>("Order not found.", StatusCodes.Status404NotFound);
                }

                // Delete related entities
                var orderItems = await _orderItemRepository.FindByOrderIdAsync(orderId);
                foreach (var orderItem in orderItems)
                {
                    await _orderItemRepository.DeleteAsync(orderItem);
                }

                var addresses = await _orderAddressRepository.FindByOrderIdAsync(orderId);
                foreach (var address in addresses)
                {
                    await _orderAddressRepository.DeleteAsync(address);
                }

                var payment = await _orderPaymentRepository.FindByOrderIdAsync(orderId);
                if (payment != null)
                {
                    await _orderPaymentRepository.DeleteAsync(payment);
                }

                await _orderRepository.DeleteAsync(order);

                return Result.Success(new DeleteOrderResponse
                {
                    ID = order.ID,
                    OrderNumber = order.OrderNumber,
                    Message = "Order deleted successfully."
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting order: {ex.Message}");
                return Result.Failure<DeleteOrderResponse>($"An error occurred while deleting the order: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateOrderResponse>> UpdateOrderStatusAsync(Guid userId, Guid orderId, string statusCode)
        {
            try
            {
                var order = await _orderRepository.FindByUserIdAndIdAsync(userId, orderId);
                if (order == null)
                {
                    return Result.Failure<UpdateOrderResponse>("Order not found.", StatusCodes.Status404NotFound);
                }

                var status = await _orderStatusRepository.FindByStatusCodeAsync(statusCode);
                if (status == null)
                {
                    return Result.Failure<UpdateOrderResponse>($"Order status '{statusCode}' not found.", StatusCodes.Status400BadRequest);
                }

                order.StatusID = status.ID;
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);

                return await BuildUpdateOrderResponse(order);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating order status: {ex.Message}");
                return Result.Failure<UpdateOrderResponse>($"An error occurred while updating the order status: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateOrderResponse>> UpdateOrderItemStatusAsync(Guid userId, Guid orderId, Guid orderItemId, string status, string? onHoldReason = null)
        {
            try
            {
                var order = await _orderRepository.FindByUserIdAndIdAsync(userId, orderId);
                if (order == null)
                {
                    return Result.Failure<UpdateOrderResponse>("Order not found.", StatusCodes.Status404NotFound);
                }

                DateTime? deliveredAt = status == "Delivered" ? DateTime.UtcNow : null;
                var success = await _orderItemRepository.UpdateStatusAsync(orderItemId, status, deliveredAt, onHoldReason);
                
                if (!success)
                {
                    return Result.Failure<UpdateOrderResponse>("Order item not found or status update failed.", StatusCodes.Status404NotFound);
                }

                // TODO: Update overall order status based on item statuses
                // This would involve complex logic to determine the overall order status

                return await BuildUpdateOrderResponse(order);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating order item status: {ex.Message}");
                return Result.Failure<UpdateOrderResponse>($"An error occurred while updating the order item status: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        // Helper methods
        private static OrderAddressResponse MapToOrderAddressResponse(OrderAddress address)
        {
            return new OrderAddressResponse
            {
                ID = address.ID,
                OrderID = address.OrderID,
                Type = address.Type,
                FullName = address.FullName,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                City = address.City,
                ProvinceState = address.ProvinceState,
                PostalCode = address.PostalCode,
                Country = address.Country
            };
        }

        private async Task<Result<GetOrderResponse>> BuildGetOrderResponse(Order order)
        {
            try
            {
                var status = await _orderStatusRepository.GetByIdAsync(Guid.Parse(order.StatusID.ToString()));
                var orderItems = await _orderItemRepository.FindByOrderIdAsync(order.ID);
                var addresses = await _orderAddressRepository.FindByOrderIdAsync(order.ID);
                var payment = await _orderPaymentRepository.FindByOrderIdAsync(order.ID);

                return Result.Success(new GetOrderResponse
                {
                    ID = order.ID,
                    UserID = order.UserID,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    StatusCode = status?.StatusCode ?? "Unknown",
                    StatusName_en = status?.Name_en ?? "Unknown",
                    StatusName_fr = status?.Name_fr ?? "Inconnu",
                    Subtotal = order.Subtotal,
                    TaxTotal = order.TaxTotal,
                    ShippingTotal = order.ShippingTotal,
                    GrandTotal = order.GrandTotal,
                    Notes = order.Notes,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    OrderItems = orderItems.Select(oi => new OrderItemResponse
                    {
                        ID = oi.ID,
                        OrderID = oi.OrderID,
                        ItemID = oi.ItemID,
                        ItemVariantID = oi.ItemVariantID,
                        Name_en = oi.Name_en,
                        Name_fr = oi.Name_fr,
                        VariantName_en = oi.VariantName_en,
                        VariantName_fr = oi.VariantName_fr,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Status = oi.Status,
                        DeliveredAt = oi.DeliveredAt,
                        OnHoldReason = oi.OnHoldReason
                    }).ToList(),
                    Addresses = addresses.Select(MapToOrderAddressResponse).ToList(),
                    Payment = payment != null ? new OrderPaymentResponse
                    {
                        ID = payment.ID,
                        OrderID = payment.OrderID,
                        PaymentMethodID = payment.PaymentMethodID,
                        Amount = payment.Amount,
                        Provider = payment.Provider,
                        ProviderReference = payment.ProviderReference,
                        PaidAt = payment.PaidAt
                    } : null
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error building order response: {ex.Message}");
                return Result.Failure<GetOrderResponse>($"An error occurred while building the order response: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<Result<UpdateOrderResponse>> BuildUpdateOrderResponse(Order order)
        {
            var getOrderResult = await BuildGetOrderResponse(order);
            if (getOrderResult.IsFailure)
            {
                return Result.Failure<UpdateOrderResponse>(getOrderResult.Error!, getOrderResult.ErrorCode ?? StatusCodes.Status500InternalServerError);
            }

            var getOrder = getOrderResult.Value!;
            return Result.Success(new UpdateOrderResponse
            {
                ID = getOrder.ID,
                UserID = getOrder.UserID,
                OrderNumber = getOrder.OrderNumber,
                OrderDate = getOrder.OrderDate,
                StatusCode = getOrder.StatusCode,
                StatusName_en = getOrder.StatusName_en,
                StatusName_fr = getOrder.StatusName_fr,
                Subtotal = getOrder.Subtotal,
                TaxTotal = getOrder.TaxTotal,
                ShippingTotal = getOrder.ShippingTotal,
                GrandTotal = getOrder.GrandTotal,
                Notes = getOrder.Notes,
                CreatedAt = getOrder.CreatedAt,
                UpdatedAt = getOrder.UpdatedAt,
                OrderItems = getOrder.OrderItems,
                Addresses = getOrder.Addresses,
                Payment = getOrder.Payment
            });
        }
    }
}