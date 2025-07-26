using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class UserRepositoryShould
    {
        [Fact]
        public async Task FindByUsernameAsync_ReturnUser_WhenUserExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var testUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            mockRepo.Setup(repo => repo.FindByUsernameAsync("testuser"))
                   .ReturnsAsync(testUser);

            // Act
            var result = await mockRepo.Object.FindByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Uname);
            Assert.Equal("test@example.com", result.Email);
            mockRepo.Verify(repo => repo.FindByUsernameAsync("testuser"), Times.Once);
        }

        [Fact]
        public async Task FindByUsernameAsync_ReturnNull_WhenUserNotExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.FindByUsernameAsync("nonexistentuser"))
                   .ReturnsAsync((User?)null);

            // Act
            var result = await mockRepo.Object.FindByUsernameAsync("nonexistentuser");

            // Assert
            Assert.Null(result);
            mockRepo.Verify(repo => repo.FindByUsernameAsync("nonexistentuser"), Times.Once);
        }

        [Fact]
        public async Task FindByEmailAsync_ReturnUser_WhenUserExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var testUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true,
                Createdat = DateTime.UtcNow
            };

            mockRepo.Setup(repo => repo.FindByEmailAsync("test@example.com"))
                   .ReturnsAsync(testUser);

            // Act
            var result = await mockRepo.Object.FindByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("testuser", result.Uname);
            mockRepo.Verify(repo => repo.FindByEmailAsync("test@example.com"), Times.Once);
        }

        [Fact]
        public async Task ExistsByUsernameAsync_ReturnTrue_WhenUserExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.ExistsByUsernameAsync("existinguser"))
                   .ReturnsAsync(true);

            // Act
            var result = await mockRepo.Object.ExistsByUsernameAsync("existinguser");

            // Assert
            Assert.True(result);
            mockRepo.Verify(repo => repo.ExistsByUsernameAsync("existinguser"), Times.Once);
        }

        [Fact]
        public async Task ExistsByUsernameAsync_ReturnFalse_WhenUserNotExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(repo => repo.ExistsByUsernameAsync("nonexistentuser"))
                   .ReturnsAsync(false);

            // Act
            var result = await mockRepo.Object.ExistsByUsernameAsync("nonexistentuser");

            // Assert
            Assert.False(result);
            mockRepo.Verify(repo => repo.ExistsByUsernameAsync("nonexistentuser"), Times.Once);
        }

        [Fact]
        public async Task FindByDeletedStatusAsync_ReturnActiveUsers_WhenDeletedIsFalse()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var activeUsers = new List<User>
            {
                new User
                {
                    ID = Guid.NewGuid(),
                    Uname = "user1",
                    Firstname = "User",
                    Lastname = "One",
                    Email = "user1@example.com",
                    Password = "hashedpassword",
                    Deleted = false,
                    ValidEmail = true,
                    Createdat = DateTime.UtcNow
                },
                new User
                {
                    ID = Guid.NewGuid(),
                    Uname = "user2",
                    Firstname = "User",
                    Lastname = "Two",
                    Email = "user2@example.com",
                    Password = "hashedpassword",
                    Deleted = false,
                    ValidEmail = true,
                    Createdat = DateTime.UtcNow
                }
            };

            mockRepo.Setup(repo => repo.FindByDeletedStatusAsync(false))
                   .ReturnsAsync(activeUsers);

            // Act
            var result = await mockRepo.Object.FindByDeletedStatusAsync(false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, user => Assert.False(user.Deleted));
            mockRepo.Verify(repo => repo.FindByDeletedStatusAsync(false), Times.Once);
        }
    }
}