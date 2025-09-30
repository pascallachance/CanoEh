using Domain.Services.Implementations;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Moq;

namespace API.Tests
{
    public class CategoryServiceExceptionHandlingShould
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CategoryService _categoryService;

        public CategoryServiceExceptionHandlingShould()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_mockCategoryRepository.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnMockData_WhenLocalDBNotSupportedException()
        {
            // Arrange
            var exception = new InvalidOperationException("LocalDB is not supported on this platform");
            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                                   .ThrowsAsync(exception);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            var categories = result.Value.ToList();
            Assert.Equal(4, categories.Count); // Should return 4 mock categories
            Assert.Contains(categories, c => c.Name_en == "Electronics");
            Assert.Contains(categories, c => c.Name_en == "Clothing");
            Assert.Contains(categories, c => c.Name_en == "Books");
            Assert.Contains(categories, c => c.Name_en == "Home & Garden");
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnMockData_WhenSQLServerLocalDBException()
        {
            // Arrange
            var exception = new InvalidOperationException("Cannot connect to SQL Server LocalDB instance");
            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                                   .ThrowsAsync(exception);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            var categories = result.Value.ToList();
            Assert.Equal(4, categories.Count); // Should return 4 mock categories
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnFailure_WhenGenericExceptionWithDatabaseInMessage()
        {
            // Arrange - This tests that the old broad check would have caught this, but new logic doesn't
            var exception = new ArgumentException("Some database related error but not LocalDB");
            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                                   .ThrowsAsync(exception);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("An error occurred while retrieving categories", result.Error);
            Assert.Contains("Some database related error but not LocalDB", result.Error);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnFailure_WhenGenericExceptionWithConnectionInMessage()
        {
            // Arrange - This tests that the old broad check would have caught this, but new logic doesn't
            var exception = new ArgumentException("Some connection error but not SqlException");
            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                                   .ThrowsAsync(exception);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("An error occurred while retrieving categories", result.Error);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnFailure_WhenUnrelatedBusinessLogicException()
        {
            // Arrange - Test that legitimate business exceptions are not masked
            var exception = new InvalidOperationException("Category validation failed");
            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                                   .ThrowsAsync(exception);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("An error occurred while retrieving categories", result.Error);
            Assert.Contains("Category validation failed", result.Error);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnFailure_WhenGenericException()
        {
            // Arrange
            var exception = new ArgumentException("Some unrelated error");
            _mockCategoryRepository.Setup(x => x.GetAllAsync())
                                   .ThrowsAsync(exception);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("An error occurred while retrieving categories", result.Error);
        }
    }
}