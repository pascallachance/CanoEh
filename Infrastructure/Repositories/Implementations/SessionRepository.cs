using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class SessionRepository(string connectionString) : GenericRepository<Session>(connectionString), ISessionRepository
    {
        public override async Task<Session> AddAsync(Session entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.Sessions (
    SessionId,
    UserId, 
    CreatedAt, 
    LoggedOutAt, 
    ExpiresAt, 
    UserAgent, 
    IpAddress)
VALUES (
    @SessionId,
    @UserId, 
    @CreatedAt, 
    @LoggedOutAt, 
    @ExpiresAt, 
    @UserAgent, 
    @IpAddress)";

            var parameters = new
            {
                entity.SessionId,
                entity.UserId,
                entity.CreatedAt,
                entity.LoggedOutAt,
                entity.ExpiresAt,
                entity.UserAgent,
                entity.IpAddress
            };
            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override async Task<Session> UpdateAsync(Session entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.Sessions
SET
    UserId = @UserId,
    CreatedAt = @CreatedAt,
    LoggedOutAt = @LoggedOutAt,
    ExpiresAt = @ExpiresAt,
    UserAgent = @UserAgent,
    IpAddress = @IpAddress
WHERE SessionId = @SessionId";

            var parameters = new
            {
                entity.SessionId,
                entity.UserId,
                entity.CreatedAt,
                entity.LoggedOutAt,
                entity.ExpiresAt,
                entity.UserAgent,
                entity.IpAddress
            };
            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override Task DeleteAsync(Session entity)
        {
            // Intentionally not implemented as per requirements
            // Sessions should not be deleted, only marked as logged out
            throw new NotSupportedException("Session deletion is not allowed. Use MarkAsLoggedOutAsync instead.");
        }

        public override async Task<Session> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Sessions 
WHERE SessionId = @id";
            return await dbConnection.QueryFirstAsync<Session>(query, new { id });
        }

        public override async Task<IEnumerable<Session>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.QueryAsync<Session>("SELECT * FROM dbo.Sessions");
        }

        public override async Task<IEnumerable<Session>> FindAsync(Func<Session, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var sessions = await dbConnection.QueryAsync<Session>("SELECT * FROM dbo.Sessions");
            return sessions.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<Session, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var sessions = await dbConnection.QueryAsync<Session>("SELECT * FROM dbo.Sessions");
            return sessions.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Sessions WHERE SessionId = @id", new { id });
        }

        // ISessionRepository specific methods
        public async Task<Session?> FindBySessionIdAsync(Guid sessionId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Sessions 
WHERE SessionId = @sessionId";
            return await dbConnection.QueryFirstOrDefaultAsync<Session>(query, new { sessionId });
        }

        public async Task<IEnumerable<Session>> FindByUserIdAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT * 
FROM dbo.Sessions 
WHERE UserId = @userId
ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<Session>(query, new { userId });
        }

        public async Task<IEnumerable<Session>> FindActiveSessionsByUserIdAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT * 
FROM dbo.Sessions 
WHERE UserId = @userId 
    AND LoggedOutAt IS NULL 
    AND ExpiresAt > @now
ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<Session>(query, new { userId, now = DateTime.UtcNow });
        }

        public async Task<Session> MarkAsLoggedOutAsync(Guid sessionId, DateTime loggedOutAt)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.Sessions
SET LoggedOutAt = @loggedOutAt
WHERE SessionId = @sessionId";

            await dbConnection.ExecuteAsync(query, new { sessionId, loggedOutAt });
            
            // Return the updated session
            return await GetByIdAsync(sessionId);
        }

        public async Task<bool> IsSessionActiveAsync(Guid sessionId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT COUNT(1) 
FROM dbo.Sessions 
WHERE SessionId = @sessionId 
    AND LoggedOutAt IS NULL 
    AND ExpiresAt > @now";
            return await dbConnection.ExecuteScalarAsync<bool>(query, new { sessionId, now = DateTime.UtcNow });
        }
    }
}