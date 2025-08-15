using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOrderStatusRepository : IRepository<OrderStatus>
    {
        Task<OrderStatus?> FindByStatusCodeAsync(string statusCode);
        Task<IEnumerable<OrderStatus>> GetAllActiveAsync();
    }
}