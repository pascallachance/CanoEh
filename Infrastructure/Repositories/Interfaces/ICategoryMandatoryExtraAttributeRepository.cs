using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICategoryMandatoryExtraAttributeRepository : IRepository<CategoryMandatoryExtraAttribute>
    {
        Task<IEnumerable<CategoryMandatoryExtraAttribute>> GetAttributesByCategoryNodeIdAsync(Guid categoryNodeId);
        Task<bool> DeleteAttributesByCategoryNodeIdAsync(Guid categoryNodeId);
    }
}
