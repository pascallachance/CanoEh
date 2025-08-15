using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<CreateOrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest createRequest);
        Task<Result<GetOrderResponse>> GetOrderAsync(Guid userId, Guid orderId);
        Task<Result<GetOrderResponse>> GetOrderByOrderNumberAsync(Guid userId, int orderNumber);
        Task<Result<IEnumerable<GetOrderResponse>>> GetUserOrdersAsync(Guid userId);
        Task<Result<IEnumerable<GetOrderResponse>>> GetUserOrdersByStatusAsync(Guid userId, string statusCode);
        Task<Result<UpdateOrderResponse>> UpdateOrderAsync(Guid userId, UpdateOrderRequest updateRequest);
        Task<Result<DeleteOrderResponse>> DeleteOrderAsync(Guid userId, Guid orderId);
        Task<Result<UpdateOrderResponse>> UpdateOrderStatusAsync(Guid userId, Guid orderId, string statusCode);
        Task<Result<UpdateOrderResponse>> UpdateOrderItemStatusAsync(Guid userId, Guid orderId, Guid orderItemId, string status, string? onHoldReason = null);
    }
}