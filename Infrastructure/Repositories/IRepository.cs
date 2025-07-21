 namespace Infrastructure.Repositories
{
    public interface IRepository<T>
    {
        T Add(T entity);

        T Update(T entity);

        void Delete(T entity);

        T GetById(Guid id);

        IEnumerable<T> GetAll();

        IEnumerable<T> Find(Func<T, bool> predicate);

        int Count(Func<T, bool> predicate);

        bool Exists(Guid id);
    }
}
