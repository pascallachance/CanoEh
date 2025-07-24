using System.Data;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories
{
    public abstract class GenericRepository<T>(string connectionString) : IRepository<T> where T : class
    {
        protected IDbConnection dbConnection = new SqlConnection(connectionString);

        public abstract T Add(T entity);

        public abstract T Update(T entity);

        public abstract void Delete(T entity);

        public abstract T GetById(Guid id);

        public abstract IEnumerable<T> GetAll();

        public abstract IEnumerable<T> Find(Func<T, bool> predicate);

        public abstract int Count(Func<T, bool> predicate);

        public abstract bool Exists(Guid id);
    }
}
