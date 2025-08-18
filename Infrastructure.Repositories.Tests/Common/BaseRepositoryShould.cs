using Infrastructure.Repositories.Interfaces;
using Moq;

namespace Infrastructure.Repositories.Tests.Common
{
    /// <summary>
    /// Base test class for testing common IRepository<T> functionality
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class BaseRepositoryShould<T> where T : class
    {
        protected readonly Mock<IRepository<T>> MockRepository;
        protected readonly string ConnectionString = "Server=test;Database=test;Trusted_Connection=true;";

        protected BaseRepositoryShould()
        {
            MockRepository = new Mock<IRepository<T>>();
        }

        [Fact]
        public async Task AddAsync_ShouldReturnEntity_WhenEntityIsValid()
        {
            // Arrange
            var entity = CreateValidEntity();
            MockRepository.Setup(repo => repo.AddAsync(entity))
                         .ReturnsAsync(entity);

            // Act
            var result = await MockRepository.Object.AddAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity, result);
            MockRepository.Verify(repo => repo.AddAsync(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedEntity_WhenEntityIsValid()
        {
            // Arrange
            var entity = CreateValidEntity();
            MockRepository.Setup(repo => repo.UpdateAsync(entity))
                         .ReturnsAsync(entity);

            // Act
            var result = await MockRepository.Object.UpdateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity, result);
            MockRepository.Verify(repo => repo.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCompleteSuccessfully_WhenEntityExists()
        {
            // Arrange
            var entity = CreateValidEntity();
            MockRepository.Setup(repo => repo.DeleteAsync(entity))
                         .Returns(Task.CompletedTask);

            // Act & Assert
            await MockRepository.Object.DeleteAsync(entity);
            MockRepository.Verify(repo => repo.DeleteAsync(entity), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenIdExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = CreateValidEntity();
            MockRepository.Setup(repo => repo.GetByIdAsync(id))
                         .ReturnsAsync(entity);

            // Act
            var result = await MockRepository.Object.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity, result);
            MockRepository.Verify(repo => repo.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCollection_WhenCalled()
        {
            // Arrange
            var entities = CreateMultipleValidEntities();
            MockRepository.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(entities);

            // Act
            var result = await MockRepository.Object.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entities.Count(), result.Count());
            MockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task FindAsync_ShouldReturnFilteredCollection_WhenPredicateProvided()
        {
            // Arrange
            var entities = CreateMultipleValidEntities();
            Func<T, bool> predicate = entity => true; // Simple predicate for testing
            MockRepository.Setup(repo => repo.FindAsync(predicate))
                         .ReturnsAsync(entities);

            // Act
            var result = await MockRepository.Object.FindAsync(predicate);

            // Assert
            Assert.NotNull(result);
            MockRepository.Verify(repo => repo.FindAsync(predicate), Times.Once);
        }

        [Fact]
        public async Task CountAsync_ShouldReturnCount_WhenPredicateProvided()
        {
            // Arrange
            var expectedCount = 5;
            Func<T, bool> predicate = entity => true;
            MockRepository.Setup(repo => repo.CountAsync(predicate))
                         .ReturnsAsync(expectedCount);

            // Act
            var result = await MockRepository.Object.CountAsync(predicate);

            // Assert
            Assert.Equal(expectedCount, result);
            MockRepository.Verify(repo => repo.CountAsync(predicate), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            MockRepository.Setup(repo => repo.ExistsAsync(id))
                         .ReturnsAsync(true);

            // Act
            var result = await MockRepository.Object.ExistsAsync(id);

            // Assert
            Assert.True(result);
            MockRepository.Verify(repo => repo.ExistsAsync(id), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            MockRepository.Setup(repo => repo.ExistsAsync(id))
                         .ReturnsAsync(false);

            // Act
            var result = await MockRepository.Object.ExistsAsync(id);

            // Assert
            Assert.False(result);
            MockRepository.Verify(repo => repo.ExistsAsync(id), Times.Once);
        }

        /// <summary>
        /// Derived classes must implement this to create a valid entity instance
        /// </summary>
        protected abstract T CreateValidEntity();

        /// <summary>
        /// Derived classes can override this to create multiple entities for collection tests
        /// </summary>
        protected virtual IEnumerable<T> CreateMultipleValidEntities()
        {
            return new List<T> { CreateValidEntity(), CreateValidEntity(), CreateValidEntity() };
        }
    }
}