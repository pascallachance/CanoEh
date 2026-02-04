using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Tests.Common;
using Moq;

namespace Infrastructure.Repositories.Tests
{
    public class CategoryMandatoryFeatureRepositoryShould : BaseRepositoryShould<CategoryMandatoryFeature>
    {
        private readonly Mock<ICategoryMandatoryFeatureRepository> _mockRepository;

        public CategoryMandatoryFeatureRepositoryShould()
        {
            _mockRepository = new Mock<ICategoryMandatoryFeatureRepository>();
        }

        protected override CategoryMandatoryFeature CreateValidEntity()
        {
            return new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = Guid.NewGuid(),
                Name_en = "Test Attribute",
                Name_fr = "Attribut de test",
                AttributeType = "string",
                SortOrder = 1
            };
        }

        protected override IEnumerable<CategoryMandatoryFeature> CreateMultipleValidEntities()
        {
            var categoryNodeId = Guid.NewGuid();
            return new List<CategoryMandatoryFeature>
            {
                new CategoryMandatoryFeature
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "Brand",
                    Name_fr = "Marque",
                    AttributeType = "string",
                    SortOrder = 1
                },
                new CategoryMandatoryFeature
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "Size",
                    Name_fr = "Taille",
                    AttributeType = "enum",
                    SortOrder = 2
                },
                new CategoryMandatoryFeature
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
        public async Task GetFeaturesByCategoryNodeIdAsync_ShouldReturnAttributes_WhenAttributesExist()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();
            var attributes = CreateMultipleValidEntities();

            _mockRepository.Setup(repo => repo.GetFeaturesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(attributes);

            // Act
            var result = await _mockRepository.Object.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attributes.Count(), result.Count());
            _mockRepository.Verify(repo => repo.GetFeaturesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }

        [Fact]
        public async Task GetFeaturesByCategoryNodeIdAsync_ShouldReturnEmpty_WhenNoAttributesExist()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();
            var emptyList = new List<CategoryMandatoryFeature>();

            _mockRepository.Setup(repo => repo.GetFeaturesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(emptyList);

            // Act
            var result = await _mockRepository.Object.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockRepository.Verify(repo => repo.GetFeaturesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }

        [Fact]
        public async Task DeleteFeaturesByCategoryNodeIdAsync_ShouldReturnTrue_WhenAttributesDeleted()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(true);

            // Act
            var result = await _mockRepository.Object.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(repo => repo.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }

        [Fact]
        public async Task DeleteFeaturesByCategoryNodeIdAsync_ShouldReturnFalse_WhenNoAttributesFound()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(false);

            // Act
            var result = await _mockRepository.Object.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.False(result);
            _mockRepository.Verify(repo => repo.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId), Times.Once);
        }
    }
}
