using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<IEnumerable<Company>> FindByOwnerAsync(Guid ownerId);
        Task<Company?> FindByNameAsync(string name);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> IsOwnerAsync(Guid companyId, Guid ownerId);
    }
}