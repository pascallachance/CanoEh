using System.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public abstract class GenericRepository<T>(string connectionString) : IRepository<T>, IDisposable where T : class
    {
        protected readonly IDbConnection dbConnection = new SqlConnection(connectionString);
        protected bool disposed = false;

        public abstract Task<T> AddAsync(T entity);

        public abstract Task<T> UpdateAsync(T entity);

        public abstract Task DeleteAsync(T entity);

        public abstract Task<T> GetByIdAsync(Guid id);

        public abstract Task<IEnumerable<T>> GetAllAsync();

        public abstract Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate);

        public abstract Task<int> CountAsync(Func<T, bool> predicate);

        public abstract Task<bool> ExistsAsync(Guid id);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources - calling Dispose() on IDbConnection automatically closes the connection if open and returns it to the pool
                    dbConnection?.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
