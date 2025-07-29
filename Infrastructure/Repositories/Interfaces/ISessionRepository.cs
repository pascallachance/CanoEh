using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ISessionRepository : IRepository<Session>
    {
        Task<Session?> FindBySessionIdAsync(Guid sessionId);
        Task<IEnumerable<Session>> FindByUserIdAsync(Guid userId);
        Task<IEnumerable<Session>> FindActiveSessionsByUserIdAsync(Guid userId);
        Task<Session> MarkAsLoggedOutAsync(Guid sessionId, DateTime loggedOutAt);
        Task<bool> IsSessionActiveAsync(Guid sessionId);
        // Note: Delete operation is intentionally not exposed as per requirements
    }
}