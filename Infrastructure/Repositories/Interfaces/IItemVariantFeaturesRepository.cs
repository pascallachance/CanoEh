using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemVariantFeaturesRepository : IRepository<ItemVariantFeatures>
    {
        Task<IEnumerable<ItemVariantFeatures>> GetFeaturesByItemVariantIdAsync(Guid itemVariantId);
        Task<bool> DeleteFeaturesByItemVariantIdAsync(Guid itemVariantId);
    }
}