using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IAddressRepository : IRepository<Address>
    {
        Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Address>> GetByUserIdAndTypeAsync(Guid userId, string addressType);
        Task<bool> ExistsByUserIdAsync(Guid userId, Guid addressId);
    }
}