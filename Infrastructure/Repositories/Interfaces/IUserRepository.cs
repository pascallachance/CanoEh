using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> FindByUsernameAsync(string username);
        Task<User?> FindByEmailAsync(string email);
        Task<IEnumerable<User>> FindByDeletedStatusAsync(bool deleted);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> FindByEmailValidationTokenAsync(string token);
    }
}