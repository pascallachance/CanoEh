using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOrderItemStatusRepository : IRepository<OrderItemStatus>
    {
        Task<OrderItemStatus?> FindByStatusCodeAsync(string statusCode);
        Task<IEnumerable<OrderItemStatus>> GetAllActiveAsync();
    }
}