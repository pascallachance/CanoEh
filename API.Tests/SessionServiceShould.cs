using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class SessionServiceShould
    {
        private readonly Mock<ISessionRepository> _mockSessionRepository;
        private readonly SessionService _sessionService;

        public SessionServiceShould()
        {
            _mockSessionRepository = new Mock<ISessionRepository>();
            _sessionService = new SessionService(_mockSessionRepository.Object);
        }

        [Fact]
        public async Task CreateSession_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userAgent = "Mozilla/5.0 Test Browser";
            var ipAddress = "192.168.1.1";
            var expectedSession = new Session
            {
                SessionId = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                UserAgent = userAgent,
                IpAddress = ipAddress
            };

            _mockSessionRepository.Setup(repo => repo.AddAsync(It.IsAny<Session>()))
                                  .ReturnsAsync(expectedSession);

            // Act
            var result = await _sessionService.CreateSessionAsync(userId, userAgent, ipAddress);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(userId, result.Value.UserId);
            Assert.Equal(userAgent, result.Value.UserAgent);
            Assert.Equal(ipAddress, result.Value.IpAddress);
            Assert.True(result.Value.IsActive);
            _mockSessionRepository.Verify(repo => repo.AddAsync(It.IsAny<Session>()), Times.Once);
        }

        [Fact]
        public async Task LogoutSession_Successfully()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var existingSession = new Session
            {
                SessionId = sessionId,
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ExpiresAt = DateTime.UtcNow.AddHours(23),
                LoggedOutAt = null
            };

            var loggedOutSession = new Session
            {
                SessionId = sessionId,
                UserId = existingSession.UserId,
                CreatedAt = existingSession.CreatedAt,
                ExpiresAt = existingSession.ExpiresAt,
                LoggedOutAt = DateTime.UtcNow
            };

            _mockSessionRepository.Setup(repo => repo.FindBySessionIdAsync(sessionId))
                                  .ReturnsAsync(existingSession);
            _mockSessionRepository.Setup(repo => repo.MarkAsLoggedOutAsync(sessionId, It.IsAny<DateTime>()))
                                  .ReturnsAsync(loggedOutSession);

            // Act
            var result = await _sessionService.LogoutSessionAsync(sessionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.LoggedOutAt);
            Assert.False(result.Value.IsActive);
            _mockSessionRepository.Verify(repo => repo.MarkAsLoggedOutAsync(sessionId, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task LogoutSession_FailWhenSessionNotFound()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _mockSessionRepository.Setup(repo => repo.FindBySessionIdAsync(sessionId))
                                  .ReturnsAsync((Session?)null);

            // Act
            var result = await _sessionService.LogoutSessionAsync(sessionId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Session not found", result.Error);
        }

        [Fact]
        public async Task LogoutSession_FailWhenAlreadyLoggedOut()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var loggedOutSession = new Session
            {
                SessionId = sessionId,
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(22),
                LoggedOutAt = DateTime.UtcNow.AddHours(-1)
            };

            _mockSessionRepository.Setup(repo => repo.FindBySessionIdAsync(sessionId))
                                  .ReturnsAsync(loggedOutSession);

            // Act
            var result = await _sessionService.LogoutSessionAsync(sessionId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Session is already logged out", result.Error);
        }

        [Fact]
        public async Task GetActiveSession_ReturnSessionWhenActive()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var activeSession = new Session
            {
                SessionId = sessionId,
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                ExpiresAt = DateTime.UtcNow.AddHours(23),
                LoggedOutAt = null
            };

            _mockSessionRepository.Setup(repo => repo.FindBySessionIdAsync(sessionId))
                                  .ReturnsAsync(activeSession);

            // Act
            var result = await _sessionService.GetActiveSessionAsync(sessionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(sessionId, result.Value.SessionId);
            Assert.True(result.Value.IsActive);
        }

        [Fact]
        public async Task GetActiveSession_ReturnNullWhenInactive()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var inactiveSession = new Session
            {
                SessionId = sessionId,
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddHours(-25),
                ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired
                LoggedOutAt = null
            };

            _mockSessionRepository.Setup(repo => repo.FindBySessionIdAsync(sessionId))
                                  .ReturnsAsync(inactiveSession);

            // Act
            var result = await _sessionService.GetActiveSessionAsync(sessionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task IsSessionActive_ReturnTrueForActiveSession()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _mockSessionRepository.Setup(repo => repo.IsSessionActiveAsync(sessionId))
                                  .ReturnsAsync(true);

            // Act
            var result = await _sessionService.IsSessionActiveAsync(sessionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task IsSessionActive_ReturnFalseForInactiveSession()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            _mockSessionRepository.Setup(repo => repo.IsSessionActiveAsync(sessionId))
                                  .ReturnsAsync(false);

            // Act
            var result = await _sessionService.IsSessionActiveAsync(sessionId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
        }
    }
}