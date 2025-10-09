using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemRepository : IRepository<Item>
    {
        Task<Item?> GetItemByIdAsync(Guid id);
    }
}