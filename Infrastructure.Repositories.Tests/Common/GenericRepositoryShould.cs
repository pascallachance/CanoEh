using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories.Tests.Common
{
    /// <summary>
    /// Tests for GenericRepository functionality
    /// </summary>
    public class GenericRepositoryShould
    {
        private readonly string ConnectionString = "Server=test;Database=test;Trusted_Connection=true;";

        [Fact]
        public void Constructor_ShouldInitializeDbConnection_WhenValidConnectionStringProvided()
        {
            // Arrange
            var testRepository = new TestGenericRepository(ConnectionString);

            // Act & Assert
            Assert.NotNull(testRepository.GetDbConnection());
        }

        [Fact]
        public void Constructor_ShouldAcceptConnectionString_WhenProvided()
        {
            // Arrange & Act
            var testRepository = new TestGenericRepository(ConnectionString);

            // Assert
            Assert.NotNull(testRepository);
        }

        [Fact]
        public void Dispose_ShouldDisposeDbConnection_WhenCalled()
        {
            // Arrange
            var testRepository = new TestGenericRepository(ConnectionString);
            var connection = testRepository.GetDbConnection();
            
            // Verify connection is initially not disposed
            Assert.NotNull(connection);
            Assert.False(testRepository.IsDisposed(), "Repository should not be disposed initially");

            // Act
            testRepository.Dispose();

            // Assert
            // Verify the disposed flag is set, indicating the connection has been disposed
            Assert.True(testRepository.IsDisposed(), "Repository should be marked as disposed after calling Dispose");
        }

        [Fact]
        public void Dispose_ShouldBeSafeToCallMultipleTimes_WhenCalledRepeatedly()
        {
            // Arrange
            var testRepository = new TestGenericRepository(ConnectionString);

            // Act - Call Dispose multiple times
            testRepository.Dispose();
            testRepository.Dispose();
            testRepository.Dispose();

            // Assert - Should not throw any exception
            Assert.True(true, "Multiple Dispose calls completed without exception");
        }

        [Fact]
        public void Dispose_ShouldSetDisposedFlag_WhenCalled()
        {
            // Arrange
            var testRepository = new TestGenericRepository(ConnectionString);

            // Act
            testRepository.Dispose();

            // Assert
            Assert.True(testRepository.IsDisposed(), "Disposed flag should be set after calling Dispose");
        }

        [Fact]
        public void UsingStatement_ShouldAutomaticallyDisposeRepository_WhenBlockEnds()
        {
            // Arrange
            TestGenericRepository? testRepository = null;

            // Act
            using (testRepository = new TestGenericRepository(ConnectionString))
            {
                // Repository is used within the using block
                Assert.NotNull(testRepository);
            }

            // Assert - Repository should be disposed after the using block
            Assert.True(testRepository.IsDisposed(), "Repository should be automatically disposed after using block");
        }

        // Helper test class that inherits from GenericRepository for testing
        private class TestGenericRepository : GenericRepository<TestEntity>
        {
            public TestGenericRepository(string connectionString) : base(connectionString) { }

            public System.Data.IDbConnection GetDbConnection() => dbConnection;

            public bool IsDisposed() => disposed;

            public override Task<TestEntity> AddAsync(TestEntity entity)
            {
                throw new NotImplementedException("Test implementation");
            }

            public override Task<TestEntity> UpdateAsync(TestEntity entity)
            {
                throw new NotImplementedException("Test implementation");
            }

            public override Task DeleteAsync(TestEntity entity)
            {
                throw new NotImplementedException("Test implementation");
            }

            public override Task<TestEntity> GetByIdAsync(Guid id)
            {
                throw new NotImplementedException("Test implementation");
            }

            public override Task<IEnumerable<TestEntity>> GetAllAsync()
            {
                throw new NotImplementedException("Test implementation");
            }

            public override Task<IEnumerable<TestEntity>> FindAsync(Func<TestEntity, bool> predicate)
            {
                throw new NotImplementedException("Test implementation");
            }

            public override Task<int> CountAsync(Func<TestEntity, bool> predicate)
            {
                throw new NotImplementedException("Test implementation");
            }

            public override Task<bool> ExistsAsync(Guid id)
            {
                throw new NotImplementedException("Test implementation");
            }
        }

        // Simple test entity for testing
        private class TestEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}