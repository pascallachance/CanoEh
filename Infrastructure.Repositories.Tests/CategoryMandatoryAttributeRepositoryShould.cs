using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Tests.Common;
using Moq;

namespace Infrastructure.Repositories.Tests
{
    public class CategoryMandatoryAttributeRepositoryShould : BaseRepositoryShould<CategoryMandatoryAttribute>
    {
        private readonly Mock<ICategoryMandatoryAttributeRepository> _mockRepository;

        public CategoryMandatoryAttributeRepositoryShould()
        {
            _mockRepository = new Mock<ICategoryMandatoryAttributeRepository>();
        }

        protected override CategoryMandatoryAttribute CreateValidEntity()
        {
            return new CategoryMandatoryAttribute
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = Guid.NewGuid(),
                Name_en = "Test Attribute",
                Name_fr = "Attribut de test",
                AttributeType = "string",
                SortOrder = 1
            };
        }

        protected override IEnumerable<CategoryMandatoryAttribute> CreateMultipleValidEntities()
        {
            var categoryNodeId = Guid.NewGuid();
            return new List<CategoryMandatoryAttribute>
            {
                new CategoryMandatoryAttribute
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "Brand",
                    Name_fr = "Marque",
                    AttributeType = "string",
                    SortOrder = 1
                },
                new CategoryMandatoryAttribute
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "Size",
                    Name_fr = "Taille",
                    AttributeType = "enum",
                    SortOrder = 2
                },
                new CategoryMandatoryAttribute
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "Weight",
                    Name_fr = "Poids",
                    AttributeType = "int",
                    SortOrder = 3
                }
            };
        }

        [Fact]
        public async Task GetAttributesByCategoryNodeIdAsync_ShouldReturnAttributes_WhenAttributesExist()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();
            var attributes = CreateMultipleValidEntities();

            _mockRepository.Setup(repo => repo.GetAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(attributes);

            // Act
            var result = await _mockRepository.Object.GetAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attributes.Count(), result.Count());
            _mockRepository.Verify(repo => repo.GetAttributesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }

        [Fact]
        public async Task GetAttributesByCategoryNodeIdAsync_ShouldReturnEmpty_WhenNoAttributesExist()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();
            var emptyList = new List<CategoryMandatoryAttribute>();

            _mockRepository.Setup(repo => repo.GetAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(emptyList);

            // Act
            var result = await _mockRepository.Object.GetAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepository.Verify(repo => repo.GetAttributesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }

        [Fact]
        public async Task DeleteAttributesByCategoryNodeIdAsync_ShouldReturnTrue_WhenAttributesDeleted()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(true);

            // Act
            var result = await _mockRepository.Object.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }

        [Fact]
        public async Task DeleteAttributesByCategoryNodeIdAsync_ShouldReturnFalse_WhenNoAttributesFound()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(false);

            // Act
            var result = await _mockRepository.Object.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(repo => repo.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }
    }
}
