using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> FindByUserIdAsync(Guid userId);
        Task<Order?> FindByUserIdAndIdAsync(Guid userId, Guid orderId);
        Task<Order?> FindByUserIdAndOrderNumberAsync(Guid userId, int orderNumber);
        Task<bool> CanUserModifyOrderAsync(Guid userId, Guid orderId);
        Task<IEnumerable<Order>> FindByUserIdAndStatusAsync(Guid userId, string statusCode);
    }
}