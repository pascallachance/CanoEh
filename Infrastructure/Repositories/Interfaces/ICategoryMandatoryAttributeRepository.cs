using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICategoryMandatoryAttributeRepository : IRepository<CategoryMandatoryAttribute>
    {
        Task<IEnumerable<CategoryMandatoryAttribute>> GetAttributesByCategoryNodeIdAsync(Guid categoryNodeId);
        Task<bool> DeleteAttributesByCategoryNodeIdAsync(Guid categoryNodeId);
    }
}
