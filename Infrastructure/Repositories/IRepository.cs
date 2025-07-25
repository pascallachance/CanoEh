 namespace Infrastructure.Repositories
{
    public interface IRepository<T>
    {
        Task<T> AddAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<T> GetByIdAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync();

        Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);

        Task<int> CountAsync(Func<T, bool> predicate);

        Task<bool> ExistsAsync(Guid id);
    }
}
