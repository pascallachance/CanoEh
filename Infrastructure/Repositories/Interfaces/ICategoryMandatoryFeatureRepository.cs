using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICategoryMandatoryFeatureRepository : IRepository<CategoryMandatoryFeature>
    {
        Task<IEnumerable<CategoryMandatoryFeature>> GetFeaturesByCategoryNodeIdAsync(Guid categoryNodeId);
        Task<bool> DeleteFeaturesByCategoryNodeIdAsync(Guid categoryNodeId);
    }
}
