using System.Data;
using System.Diagnostics;
using Dapper;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

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
        private readonly ITaxRatesService _taxRatesService;
        private readonly string _connectionString;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IOrderAddressRepository orderAddressRepository,
            IOrderPaymentRepository orderPaymentRepository,
            IOrderStatusRepository orderStatusRepository,
            IItemRepository itemRepository,
            IUserRepository userRepository,
            ITaxRatesService taxRatesService,
            string connectionString)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderAddressRepository = orderAddressRepository;
            _orderPaymentRepository = orderPaymentRepository;
            _orderStatusRepository = orderStatusRepository;
            _itemRepository = itemRepository;
            _userRepository = userRepository;
            _taxRatesService = taxRatesService;
            _connectionString = connectionString;
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

                // Calculate tax and shipping using tax rates from TaxRate table
                decimal taxTotal = 0;
                var taxRatesResult = await _taxRatesService.GetTaxRatesByLocationAsync(
                    createRequest.BillingAddress.Country,
                    createRequest.BillingAddress.ProvinceState);
                
                if (taxRatesResult.IsSuccess && taxRatesResult.Value != null)
                {
                    var activeTaxRates = taxRatesResult.Value.Where(tr => tr.IsActive);
                    var totalTaxRate = activeTaxRates.Sum(tr => tr.Rate);
                    taxTotal = subtotal * totalTaxRate;
                }
                // If no tax rates found, taxTotal remains 0 (no tax)

                decimal shippingRate = 10.00m; // Example shipping rate
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

                // Create addresses
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

                // Create payment record
                var payment = new OrderPayment
                {
                    ID = Guid.NewGuid(),
                    OrderID = orderId,
                    PaymentMethodID = createRequest.Payment.PaymentMethodID,
                    Amount = grandTotal,
                    Provider = createRequest.Payment.Provider
                };

                // Execute all database operations in a single transaction
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Insert order
                    var orderQuery = @"
INSERT INTO dbo.[Order] (
    ID,
    UserID,
    OrderDate,
    StatusID,
    Subtotal,
    TaxTotal,
    ShippingTotal,
    GrandTotal,
    Notes,
    CreatedAt,
    UpdatedAt)
OUTPUT INSERTED.OrderNumber
VALUES (
    @ID,
    @UserID,
    @OrderDate,
    @StatusID,
    @Subtotal,
    @TaxTotal,
    @ShippingTotal,
    @GrandTotal,
    @Notes,
    @CreatedAt,
    @UpdatedAt)";

                    var orderNumber = await connection.QuerySingleAsync<int>(orderQuery, order, transaction);
                    order.OrderNumber = orderNumber;

                    // Insert order items
                    var orderItemQuery = @"
INSERT INTO dbo.OrderItem (
    ID,
    OrderID,
    ItemID,
    ItemVariantID,
    Name_en,
    Name_fr,
    VariantName_en,
    VariantName_fr,
    Quantity,
    UnitPrice,
    TotalPrice,
    Status)
VALUES (
    @ID,
    @OrderID,
    @ItemID,
    @ItemVariantID,
    @Name_en,
    @Name_fr,
    @VariantName_en,
    @VariantName_fr,
    @Quantity,
    @UnitPrice,
    @TotalPrice,
    @Status)";

                    foreach (var orderItem in orderItems)
                    {
                        await connection.ExecuteAsync(orderItemQuery, orderItem, transaction);
                    }

                    // Insert addresses
                    var addressQuery = @"
INSERT INTO dbo.OrderAddress (
    ID,
    OrderID,
    Type,
    FullName,
    AddressLine1,
    AddressLine2,
    AddressLine3,
    City,
    ProvinceState,
    PostalCode,
    Country)
VALUES (
    @ID,
    @OrderID,
    @Type,
    @FullName,
    @AddressLine1,
    @AddressLine2,
    @AddressLine3,
    @City,
    @ProvinceState,
    @PostalCode,
    @Country)";

                    await connection.ExecuteAsync(addressQuery, shippingAddress, transaction);
                    await connection.ExecuteAsync(addressQuery, billingAddress, transaction);

                    // Insert payment
                    var paymentQuery = @"
INSERT INTO dbo.OrderPayment (
    ID,
    OrderID,
    PaymentMethodID,
    Amount,
    Provider,
    ProviderReference,
    PaidAt)
VALUES (
    @ID,
    @OrderID,
    @PaymentMethodID,
    @Amount,
    @Provider,
    @ProviderReference,
    @PaidAt)";

                    await connection.ExecuteAsync(paymentQuery, payment, transaction);

                    // Decrease stock quantities
                    var stockUpdateQuery = @"
UPDATE dbo.ItemVariants 
SET StockQuantity = StockQuantity - @Quantity 
WHERE Id = @ItemVariantID";

                    foreach (var orderItem in orderItems)
                    {
                        await connection.ExecuteAsync(stockUpdateQuery, new { orderItem.Quantity, orderItem.ItemVariantID }, transaction);
                    }

                    // Commit transaction - all operations succeeded
                    transaction.Commit();

                    return Result.Success(new CreateOrderResponse
                    {
                        ID = order.ID,
                        UserID = order.UserID,
                        OrderNumber = order.OrderNumber,
                        OrderDate = order.OrderDate,
                        StatusCode = pendingStatus.StatusCode,
                        StatusName_en = pendingStatus.Name_en,
                        StatusName_fr = pendingStatus.Name_fr,
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
                catch
                {
                    // Rollback transaction on any failure
                    transaction.Rollback();
                    throw;
                }
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

                // Validate status if provided
                OrderStatus? newStatus = null;
                if (!string.IsNullOrWhiteSpace(updateRequest.StatusCode))
                {
                    newStatus = await _orderStatusRepository.FindByStatusCodeAsync(updateRequest.StatusCode);
                    if (newStatus == null)
                    {
                        return Result.Failure<UpdateOrderResponse>($"Order status '{updateRequest.StatusCode}' not found.", StatusCodes.Status400BadRequest);
                    }
                }

                // Validate order items if provided
                var orderItemsToUpdate = new List<(OrderItem orderItem, UpdateOrderItemRequest updateRequest)>();
                if (updateRequest.OrderItems != null)
                {
                    foreach (var orderItemUpdate in updateRequest.OrderItems)
                    {
                        var orderItem = await _orderItemRepository.GetByIdAsync(orderItemUpdate.ID);
                        if (orderItem == null || orderItem.OrderID != order.ID)
                        {
                            return Result.Failure<UpdateOrderResponse>($"Order item with ID {orderItemUpdate.ID} not found in this order.", StatusCodes.Status404NotFound);
                        }
                        orderItemsToUpdate.Add((orderItem, orderItemUpdate));
                    }
                }

                // Execute all database operations in a single transaction
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Update order items if provided
                    if (updateRequest.OrderItems != null)
                    {
                        foreach (var (orderItem, orderItemUpdate) in orderItemsToUpdate)
                        {
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

                            // Update order item in transaction
                            var orderItemQuery = @"
UPDATE dbo.OrderItem SET
    Quantity = @Quantity,
    UnitPrice = @UnitPrice,
    TotalPrice = @TotalPrice,
    Status = @Status,
    DeliveredAt = @DeliveredAt,
    OnHoldReason = @OnHoldReason
WHERE ID = @ID";

                            await connection.ExecuteAsync(orderItemQuery, orderItem, transaction);
                        }

                        // Recalculate order totals if quantities changed
                        var orderItemsQuery = "SELECT * FROM dbo.OrderItem WHERE OrderID = @orderId";
                        var allOrderItems = await connection.QueryAsync<OrderItem>(orderItemsQuery, new { orderId = order.ID }, transaction);
                        order.Subtotal = allOrderItems.Sum(oi => oi.TotalPrice);
                        
                        // Get billing address to calculate tax
                        var billingAddressQuery = "SELECT * FROM dbo.OrderAddress WHERE OrderID = @orderId AND Type = 'Billing'";
                        var billingAddress = await connection.QueryFirstOrDefaultAsync<OrderAddress>(billingAddressQuery, new { orderId = order.ID }, transaction);
                        
                        decimal taxTotal = 0;
                        if (billingAddress != null)
                        {
                            var taxRatesResult = await _taxRatesService.GetTaxRatesByLocationAsync(
                                billingAddress.Country,
                                billingAddress.ProvinceState);
                            
                            if (taxRatesResult.IsSuccess && taxRatesResult.Value != null)
                            {
                                var activeTaxRates = taxRatesResult.Value.Where(tr => tr.IsActive);
                                var totalTaxRate = activeTaxRates.Sum(tr => tr.Rate);
                                taxTotal = order.Subtotal * totalTaxRate;
                            }
                        }
                        
                        order.TaxTotal = taxTotal;
                        order.GrandTotal = order.Subtotal + order.TaxTotal + order.ShippingTotal;
                    }

                    // Update status if provided
                    if (newStatus != null)
                    {
                        order.StatusID = newStatus.ID;
                    }

                    // Update notes if provided
                    if (updateRequest.Notes != null)
                    {
                        order.Notes = updateRequest.Notes;
                    }

                    order.UpdatedAt = DateTime.UtcNow;

                    // Update order in transaction
                    var orderQuery = @"
UPDATE dbo.[Order] SET
    StatusID = @StatusID,
    Subtotal = @Subtotal,
    TaxTotal = @TaxTotal,
    ShippingTotal = @ShippingTotal,
    GrandTotal = @GrandTotal,
    Notes = @Notes,
    UpdatedAt = @UpdatedAt
WHERE ID = @ID";

                    await connection.ExecuteAsync(orderQuery, order, transaction);

                    // Commit transaction - all operations succeeded
                    transaction.Commit();

                    return await BuildUpdateOrderResponse(order);
                }
                catch
                {
                    // Rollback transaction on any failure
                    transaction.Rollback();
                    throw;
                }
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

                // Execute all database operations in a single transaction
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Delete child records first to respect foreign key constraints
                    
                    // Delete order items
                    var deleteOrderItemsQuery = "DELETE FROM dbo.OrderItem WHERE OrderID = @orderId";
                    await connection.ExecuteAsync(deleteOrderItemsQuery, new { orderId }, transaction);

                    // Delete order addresses
                    var deleteOrderAddressesQuery = "DELETE FROM dbo.OrderAddress WHERE OrderID = @orderId";
                    await connection.ExecuteAsync(deleteOrderAddressesQuery, new { orderId }, transaction);

                    // Delete order payment
                    var deleteOrderPaymentQuery = "DELETE FROM dbo.OrderPayment WHERE OrderID = @orderId";
                    await connection.ExecuteAsync(deleteOrderPaymentQuery, new { orderId }, transaction);

                    // Delete the order itself
                    var deleteOrderQuery = "DELETE FROM dbo.[Order] WHERE ID = @orderId";
                    await connection.ExecuteAsync(deleteOrderQuery, new { orderId }, transaction);

                    // Commit transaction - all operations succeeded
                    transaction.Commit();

                    return Result.Success(new DeleteOrderResponse
                    {
                        ID = order.ID,
                        OrderNumber = order.OrderNumber,
                        Message = "Order deleted successfully."
                    });
                }
                catch
                {
                    // Rollback transaction on any failure
                    transaction.Rollback();
                    throw;
                }
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