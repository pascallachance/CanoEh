using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICategoryNodeRepository : IRepository<BaseNode>
    {
        Task<BaseNode?> GetNodeByIdAsync(Guid id);
        Task<IEnumerable<BaseNode>> GetRootNodesAsync(); // Get all Departement nodes
        Task<IEnumerable<BaseNode>> GetChildrenAsync(Guid parentId); // Get children of a node
        Task<bool> HasChildrenAsync(Guid nodeId);
        Task<bool> HasItemsAsync(Guid categoryNodeId); // Check if a CategoryNode has items
        Task<IEnumerable<BaseNode>> GetNodesByTypeAsync(string nodeType);
        Task<IEnumerable<BaseNode>> GetCategoryNodesAsync(); // Get all CategoryNode items
        Task<(BaseNode node, IEnumerable<CategoryMandatoryAttribute> attributes)> AddNodeWithAttributesAsync(
            BaseNode node, 
            IEnumerable<CategoryMandatoryAttribute> attributes);
    }
}
