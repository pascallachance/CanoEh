using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOrderAddressRepository : IRepository<OrderAddress>
    {
        Task<IEnumerable<OrderAddress>> FindByOrderIdAsync(Guid orderId);
        Task<OrderAddress?> FindByOrderIdAndTypeAsync(Guid orderId, string type);
    }
}