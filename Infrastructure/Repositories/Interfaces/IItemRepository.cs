using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemRepository : IRepository<Item>
    {
        Task<Item?> GetItemByIdAsync(Guid id);
        Task<IEnumerable<Item>> GetBySellerIdAsync(Guid sellerId, bool includeDeleted = false);
        Task<IEnumerable<Item>> GetRecentlyAddedProductsAsync(int count = 100);
        Task<IEnumerable<Item>> GetSuggestedProductsAsync(int count = 4);
    }
}