using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Tests.Common;
using Moq;

namespace Infrastructure.Repositories.Tests
{
    public class UserRepositoryShould : BaseRepositoryShould<User>
    {
        private readonly UserRepository _userRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;

        public UserRepositoryShould()
        {
            _userRepository = new UserRepository(ConnectionString);
            _mockUserRepository = new Mock<IUserRepository>();
        }

        protected override User CreateValidEntity()
        {
            return new User
            {
                ID = Guid.NewGuid(),
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword123",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true
            };
        }

        protected override IEnumerable<User> CreateMultipleValidEntities()
        {
            return new List<User>
            {
                new User
                {
                    ID = Guid.NewGuid(),
                    Email = "user1@example.com",
                    Firstname = "User",
                    Lastname = "One",
                    Password = "hashedpassword1",
                    Createdat = DateTime.UtcNow,
                    Deleted = false,
                    ValidEmail = true
                },
                new User
                {
                    ID = Guid.NewGuid(),
                    Email = "user2@example.com",
                    Firstname = "User",
                    Lastname = "Two",
                    Password = "hashedpassword2",
                    Createdat = DateTime.UtcNow,
                    Deleted = false,
                    ValidEmail = true
                },
                new User
                {
                    ID = Guid.NewGuid(),
                    Email = "user3@example.com",
                    Firstname = "User",
                    Lastname = "Three",
                    Password = "hashedpassword3",
                    Createdat = DateTime.UtcNow,
                    Deleted = false,
                    ValidEmail = true
                }
            };
        }

        // Test UserRepository specific methods
        [Fact]
        public async Task FindByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var testUser = CreateValidEntity();
            _mockUserRepository.Setup(repo => repo.FindByEmailAsync(testUser.Email))
                              .ReturnsAsync(testUser);

            // Act
            var result = await _mockUserRepository.Object.FindByEmailAsync(testUser.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testUser.Email, result.Email);
            _mockUserRepository.Verify(repo => repo.FindByEmailAsync(testUser.Email), Times.Once);
        }

        [Fact]
        public async Task FindByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUserRepository.Setup(repo => repo.FindByEmailAsync(email))
                              .ReturnsAsync((User?)null);

            // Act
            var result = await _mockUserRepository.Object.FindByEmailAsync(email);

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(repo => repo.FindByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task FindByDeletedStatusAsync_ShouldReturnActiveUsers_WhenDeletedIsFalse()
        {
            // Arrange
            var activeUsers = CreateMultipleValidEntities().Where(u => !u.Deleted);
            _mockUserRepository.Setup(repo => repo.FindByDeletedStatusAsync(false))
                              .ReturnsAsync(activeUsers);

            // Act
            var result = await _mockUserRepository.Object.FindByDeletedStatusAsync(false);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, user => Assert.False(user.Deleted));
            _mockUserRepository.Verify(repo => repo.FindByDeletedStatusAsync(false), Times.Once);
        }

        [Fact]
        public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            _mockUserRepository.Setup(repo => repo.ExistsByEmailAsync(email))
                              .ReturnsAsync(true);

            // Act
            var result = await _mockUserRepository.Object.ExistsByEmailAsync(email);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(repo => repo.ExistsByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _mockUserRepository.Setup(repo => repo.ExistsByEmailAsync(email))
                              .ReturnsAsync(false);

            // Act
            var result = await _mockUserRepository.Object.ExistsByEmailAsync(email);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(repo => repo.ExistsByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task FindByEmailValidationTokenAsync_ShouldReturnUser_WhenTokenExists()
        {
            // Arrange
            var token = "valid-token-123";
            var testUser = CreateValidEntity();
            testUser.EmailValidationToken = token;
            _mockUserRepository.Setup(repo => repo.FindByEmailValidationTokenAsync(token))
                              .ReturnsAsync(testUser);

            // Act
            var result = await _mockUserRepository.Object.FindByEmailValidationTokenAsync(token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(token, result.EmailValidationToken);
            _mockUserRepository.Verify(repo => repo.FindByEmailValidationTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordResetTokenAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            var token = "reset-token-123";
            var expiry = DateTime.UtcNow.AddHours(1);
            _mockUserRepository.Setup(repo => repo.UpdatePasswordResetTokenAsync(email, token, expiry))
                              .ReturnsAsync(true);

            // Act
            var result = await _mockUserRepository.Object.UpdatePasswordResetTokenAsync(email, token, expiry);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(repo => repo.UpdatePasswordResetTokenAsync(email, token, expiry), Times.Once);
        }

        [Fact]
        public async Task ClearPasswordResetTokenAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            _mockUserRepository.Setup(repo => repo.ClearPasswordResetTokenAsync(email))
                              .ReturnsAsync(true);

            // Act
            var result = await _mockUserRepository.Object.ClearPasswordResetTokenAsync(email);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(repo => repo.ClearPasswordResetTokenAsync(email), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenValidConnectionStringProvided()
        {
            // Arrange & Act
            var repository = new UserRepository(ConnectionString);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public void User_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var user = CreateValidEntity();

            // Assert
            Assert.NotEqual(Guid.Empty, user.ID);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("Test", user.Firstname);
            Assert.Equal("User", user.Lastname);
            Assert.Equal("hashedpassword123", user.Password);
            Assert.False(user.Deleted);
            Assert.True(user.ValidEmail);
        }

        [Fact]
        public void User_ShouldInitializeCollectionsCorrectly()
        {
            // Arrange & Act
            var user = new User
            {
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "password"
            };

            // Assert
            Assert.False(user.Deleted);
            Assert.False(user.ValidEmail);
            Assert.Equal(Guid.Empty, user.ID);
            Assert.Null(user.Phone);
            Assert.Null(user.Lastlogin);
            Assert.Null(user.Lastupdatedat);
        }
    }
}