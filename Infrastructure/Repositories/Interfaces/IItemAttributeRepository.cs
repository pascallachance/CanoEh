using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemAttributeRepository : IRepository<ItemAttribute>
    {
        Task<IEnumerable<ItemAttribute>> GetAttributesByItemIdAsync(Guid itemId);
        Task<bool> DeleteAttributesByItemIdAsync(Guid itemId);
    }
}