using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemVariantExtraAttributeRepository : IRepository<ItemVariantExtraAttribute>
    {
        Task<IEnumerable<ItemVariantExtraAttribute>> GetAttributesByVariantIdAsync(Guid variantId);
        Task<bool> DeleteAttributesByVariantIdAsync(Guid variantId);
    }
}
