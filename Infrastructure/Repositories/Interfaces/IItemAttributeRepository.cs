using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IItemAttributeRepository : IRepository<ItemAttribute>
    {
        Task<IEnumerable<ItemAttribute>> GetAttributesByItemVariantIdAsync(Guid itemVariantId);
        Task<bool> DeleteAttributesByItemVariantIdAsync(Guid itemVariantId);
    }
}