using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> FindByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> FindByOrderIdAndStatusAsync(Guid orderId, string status);
        Task<bool> UpdateStatusAsync(Guid orderItemId, string status, DateTime? deliveredAt = null, string? onHoldReason = null);
        Task<bool> UpdateStatusBulkAsync(IEnumerable<Guid> orderItemIds, string status, DateTime? deliveredAt = null, string? onHoldReason = null);
    }
}