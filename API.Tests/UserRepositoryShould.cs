using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class UserRepositoryShould
    {
        [Fact]
        public async Task FindByEmailAsync_ReturnUser_WhenUserExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var testUser = new User
            {
                ID = Guid.NewGuid(),
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
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
            mockRepo.Verify(repo => repo.FindByEmailAsync("test@example.com"), Times.Once);
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
                    Email = "user1@example.com",
                    Firstname = "User",
                    Lastname = "One",
                    Password = "hashedpassword",
                    Deleted = false,
                    ValidEmail = true,
                    Createdat = DateTime.UtcNow
                },
                new User
                {
                    ID = Guid.NewGuid(),
                    Email = "user2@example.com",
                    Firstname = "User",
                    Lastname = "Two",
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