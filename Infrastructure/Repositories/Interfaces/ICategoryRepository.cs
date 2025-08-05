using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubcategoriesAsync(Guid parentCategoryId);
        Task<bool> HasSubcategoriesAsync(Guid categoryId);
        Task<bool> HasItemsAsync(Guid categoryId);
    }
}