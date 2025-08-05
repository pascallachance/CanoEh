using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemRepository : IRepository<Item>
    {
        Task<Item?> GetItemByIdAsync(Guid id);
        Task<bool> DeleteItemVariantAsync(Guid itemId, Guid variantId);
    }
}