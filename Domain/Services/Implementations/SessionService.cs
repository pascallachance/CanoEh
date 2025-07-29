using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class SessionService(ISessionRepository sessionRepository) : ISessionService
    {
        private readonly ISessionRepository _sessionRepository = sessionRepository;

        public async Task<Result<Session>> CreateSessionAsync(Guid userId, string? userAgent = null, string? ipAddress = null)
        {
            try
            {
                var session = new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    LoggedOutAt = null,
                    ExpiresAt = DateTime.UtcNow.AddHours(24), // Default 24 hour session
                    UserAgent = userAgent,
                    IpAddress = ipAddress
                };

                var createdSession = await _sessionRepository.AddAsync(session);
                return Result.Success(createdSession);
            }
            catch (Exception ex)
            {
                return Result.Failure<Session>($"Failed to create session: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<Session>> LogoutSessionAsync(Guid sessionId)
        {
            try
            {
                var session = await _sessionRepository.FindBySessionIdAsync(sessionId);
                if (session == null)
                {
                    return Result.Failure<Session>("Session not found", StatusCodes.Status404NotFound);
                }

                if (session.LoggedOutAt != null)
                {
                    return Result.Failure<Session>("Session is already logged out", StatusCodes.Status400BadRequest);
                }

                var loggedOutSession = await _sessionRepository.MarkAsLoggedOutAsync(sessionId, DateTime.UtcNow);
                return Result.Success(loggedOutSession);
            }
            catch (Exception ex)
            {
                return Result.Failure<Session>($"Failed to logout session: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<Session?>> GetActiveSessionAsync(Guid sessionId)
        {
            try
            {
                var session = await _sessionRepository.FindBySessionIdAsync(sessionId);
                if (session == null)
                {
                    return Result.Success<Session?>(null);
                }

                // Return session only if it's active
                if (session.IsActive)
                {
                    return Result.Success<Session?>(session);
                }

                return Result.Success<Session?>(null);
            }
            catch (Exception ex)
            {
                return Result.Failure<Session?>($"Failed to get session: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<Session>>> GetUserActiveSessionsAsync(Guid userId)
        {
            try
            {
                var activeSessions = await _sessionRepository.FindActiveSessionsByUserIdAsync(userId);
                return Result.Success(activeSessions);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<Session>>($"Failed to get user sessions: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<bool>> IsSessionActiveAsync(Guid sessionId)
        {
            try
            {
                var isActive = await _sessionRepository.IsSessionActiveAsync(sessionId);
                return Result.Success(isActive);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Failed to check session status: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }
    }
}