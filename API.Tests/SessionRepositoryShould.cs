using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;

namespace API.Tests
{
    public class SessionRepositoryShould
    {
        private readonly SessionRepository _sessionRepository;
        private readonly string _connectionString = "Server=test;Database=test;Trusted_Connection=true;";

        public SessionRepositoryShould()
        {
            _sessionRepository = new SessionRepository(_connectionString);
        }

        [Fact]
        public void DeleteAsync_ThrowNotSupportedException()
        {
            // Arrange
            var session = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<NotSupportedException>(() => _sessionRepository.DeleteAsync(session));
            Assert.Equal("Session deletion is not allowed. Use MarkAsLoggedOutAsync instead.", exception.Result.Message);
        }

        [Fact]
        public void Session_IsActive_PropertyWorksCorrectly()
        {
            // Arrange & Act
            var activeSession = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                LoggedOutAt = null
            };

            var expiredSession = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-30),
                LoggedOutAt = null
            };

            var loggedOutSession = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                LoggedOutAt = DateTime.UtcNow.AddMinutes(-10)
            };

            // Assert
            Assert.True(activeSession.IsActive);
            Assert.False(expiredSession.IsActive);
            Assert.False(loggedOutSession.IsActive);
        }
    }
}