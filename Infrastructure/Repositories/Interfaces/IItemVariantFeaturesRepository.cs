using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemVariantFeaturesRepository : IRepository<ItemVariantFeatures>
    {
        Task<IEnumerable<ItemVariantFeatures>> GetAttributesByItemVariantIdAsync(Guid itemVariantId);
        Task<bool> DeleteAttributesByItemVariantIdAsync(Guid itemVariantId);
    }
}