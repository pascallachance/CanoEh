using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemVariantAttributeRepository : IRepository<ItemVariantAttribute>
    {
        Task<IEnumerable<ItemVariantAttribute>> GetAttributesByVariantIdAsync(Guid variantId);
        Task<bool> DeleteAttributesByVariantIdAsync(Guid variantId);
    }
}