using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> FindByEmailAsync(string email);
        Task<IEnumerable<User>> FindByDeletedStatusAsync(bool deleted);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User?> FindByEmailValidationTokenAsync(string token);
        Task<User?> FindByPasswordResetTokenAsync(string token);
        Task<bool> UpdatePasswordResetTokenAsync(string email, string token, DateTime expiry);
        Task<bool> ClearPasswordResetTokenAsync(string email);
        Task<User?> FindDeletedByEmailAsync(string email);
        Task<User?> FindByRestoreUserTokenAsync(string token);
        Task<bool> UpdateRestoreUserTokenAsync(string email, string token, DateTime expiry);
        Task<bool> RestoreUserByTokenAsync(string token);
        Task<User?> FindByRefreshTokenAsync(string refreshToken);
        Task<bool> UpdateRefreshTokenAsync(string email, string refreshToken, DateTime expiry);
        Task<bool> ClearRefreshTokenAsync(string email);
    }
}