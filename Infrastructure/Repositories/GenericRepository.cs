using System.Data;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public abstract class GenericRepository<T>(string connectionString) : IRepository<T> where T : class
    {
        protected IDbConnection dbConnection = new SqlConnection(connectionString);

        public abstract Task<T> AddAsync(T entity);

        public abstract Task<T> UpdateAsync(T entity);

        public abstract Task DeleteAsync(T entity);

        public abstract Task<T> GetByIdAsync(Guid id);

        public abstract Task<IEnumerable<T>> GetAllAsync();

        public abstract Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);

        public abstract Task<int> CountAsync(Func<T, bool> predicate);

        public abstract Task<bool> ExistsAsync(Guid id);
    }
}
