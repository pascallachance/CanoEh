using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Tests.Common;
using Moq;

namespace Infrastructure.Repositories.Tests
{
    public class SessionRepositoryShould : BaseRepositoryShould<Session>
    {
        private readonly SessionRepository _sessionRepository;
        private readonly Mock<ISessionRepository> _mockSessionRepository;

        public SessionRepositoryShould()
        {
            _sessionRepository = new SessionRepository(ConnectionString);
            _mockSessionRepository = new Mock<ISessionRepository>();
        }

        protected override Session CreateValidEntity()
        {
            return new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                UserAgent = "Test-Agent/1.0",
                IpAddress = "127.0.0.1"
            };
        }

        protected override IEnumerable<Session> CreateMultipleValidEntities()
        {
            return new List<Session>
            {
                new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    UserAgent = "Test-Agent/1.0",
                    IpAddress = "127.0.0.1"
                },
                new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(12),
                    UserAgent = "Test-Agent/2.0",
                    IpAddress = "192.168.1.1"
                },
                new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    UserAgent = "Test-Agent/3.0",
                    IpAddress = "10.0.0.1"
                }
            };
        }

        // Test SessionRepository specific methods
        [Fact]
        public async Task FindBySessionIdAsync_ShouldReturnSession_WhenSessionExists()
        {
            // Arrange
            var session = CreateValidEntity();
            _mockSessionRepository.Setup(repo => repo.FindBySessionIdAsync(session.SessionId))
                                 .ReturnsAsync(session);

            // Act
            var result = await _mockSessionRepository.Object.FindBySessionIdAsync(session.SessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(session.SessionId, result.SessionId);
            _mockSessionRepository.Verify(repo => repo.FindBySessionIdAsync(session.SessionId), Times.Once);
        }

        [Fact]
        public async Task FindByUserIdAsync_ShouldReturnUserSessions_WhenUserHasSessions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessions = CreateMultipleValidEntities().ToList();
            sessions.ForEach(s => s.UserId = userId);
            
            _mockSessionRepository.Setup(repo => repo.FindByUserIdAsync(userId))
                                 .ReturnsAsync(sessions);

            // Act
            var result = await _mockSessionRepository.Object.FindByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, session => Assert.Equal(userId, session.UserId));
            _mockSessionRepository.Verify(repo => repo.FindByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task FindActiveSessionsByUserIdAsync_ShouldReturnActiveSessions_WhenUserHasActiveSessions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var activeSessions = new List<Session>
            {
                new Session
                {
                    SessionId = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    LoggedOutAt = null // Active session
                }
            };

            _mockSessionRepository.Setup(repo => repo.FindActiveSessionsByUserIdAsync(userId))
                                 .ReturnsAsync(activeSessions);

            // Act
            var result = await _mockSessionRepository.Object.FindActiveSessionsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, session => Assert.True(session.IsActive));
            _mockSessionRepository.Verify(repo => repo.FindActiveSessionsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task MarkAsLoggedOutAsync_ShouldReturnUpdatedSession_WhenSessionExists()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var loggedOutAt = DateTime.UtcNow;
            var session = CreateValidEntity();
            session.SessionId = sessionId;
            session.LoggedOutAt = loggedOutAt;

            _mockSessionRepository.Setup(repo => repo.MarkAsLoggedOutAsync(sessionId, loggedOutAt))
                                 .ReturnsAsync(session);

            // Act
            var result = await _mockSessionRepository.Object.MarkAsLoggedOutAsync(sessionId, loggedOutAt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loggedOutAt, result.LoggedOutAt);
            _mockSessionRepository.Verify(repo => repo.MarkAsLoggedOutAsync(sessionId, loggedOutAt), Times.Once);
        }

        [Fact]
        public async Task IsSessionActiveAsync_ShouldReturnTrue_WhenSessionIsActive()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _mockSessionRepository.Setup(repo => repo.IsSessionActiveAsync(sessionId))
                                 .ReturnsAsync(true);

            // Act
            var result = await _mockSessionRepository.Object.IsSessionActiveAsync(sessionId);

            // Assert
            Assert.True(result);
            _mockSessionRepository.Verify(repo => repo.IsSessionActiveAsync(sessionId), Times.Once);
        }

        [Fact]
        public async Task IsSessionActiveAsync_ShouldReturnFalse_WhenSessionIsNotActive()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _mockSessionRepository.Setup(repo => repo.IsSessionActiveAsync(sessionId))
                                 .ReturnsAsync(false);

            // Act
            var result = await _mockSessionRepository.Object.IsSessionActiveAsync(sessionId);

            // Assert
            Assert.False(result);
            _mockSessionRepository.Verify(repo => repo.IsSessionActiveAsync(sessionId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowNotSupportedException()
        {
            // Arrange
            var session = CreateValidEntity();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotSupportedException>(() => _sessionRepository.DeleteAsync(session));
            Assert.Equal("Session deletion is not allowed. Use MarkAsLoggedOutAsync instead.", exception.Message);
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

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenValidConnectionStringProvided()
        {
            // Arrange & Act
            var repository = new SessionRepository(ConnectionString);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public void Session_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var session = CreateValidEntity();

            // Assert
            Assert.NotEqual(Guid.Empty, session.SessionId);
            Assert.NotEqual(Guid.Empty, session.UserId);
            Assert.True(session.CreatedAt <= DateTime.UtcNow);
            Assert.True(session.ExpiresAt > DateTime.UtcNow);
            Assert.Equal("Test-Agent/1.0", session.UserAgent);
            Assert.Equal("127.0.0.1", session.IpAddress);
        }
    }
}