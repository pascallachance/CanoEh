using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemVariantRepository : IRepository<ItemVariant>
    {
        Task<bool> DeleteItemVariantAsync(Guid itemId, Guid variantId);
    }
}
