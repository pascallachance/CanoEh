using Infrastructure.Data;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface ISessionService
    {
        Task<Result<Session>> CreateSessionAsync(Guid userId, string? userAgent = null, string? ipAddress = null);
        Task<Result<Session>> LogoutSessionAsync(Guid sessionId);
        Task<Result<Session?>> GetActiveSessionAsync(Guid sessionId);
        Task<Result<IEnumerable<Session>>> GetUserActiveSessionsAsync(Guid userId);
        Task<Result<bool>> IsSessionActiveAsync(Guid sessionId);
    }
}