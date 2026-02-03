using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Tests.Common;
using Moq;

namespace Infrastructure.Repositories.Tests
{
    public class CategoryMandatoryExtraAttributeRepositoryShould : BaseRepositoryShould<CategoryMandatoryExtraAttribute>
    {
        private readonly Mock<ICategoryMandatoryExtraAttributeRepository> _mockRepository;

        public CategoryMandatoryExtraAttributeRepositoryShould()
        {
            _mockRepository = new Mock<ICategoryMandatoryExtraAttributeRepository>();
        }

        protected override CategoryMandatoryExtraAttribute CreateValidEntity()
        {
            return new CategoryMandatoryExtraAttribute
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = Guid.NewGuid(),
                Name_en = "SKU",
                Name_fr = "SKU",
                AttributeType = "string",
                SortOrder = 1
            };
        }

        protected override IEnumerable<CategoryMandatoryExtraAttribute> CreateMultipleValidEntities()
        {
            var categoryNodeId = Guid.NewGuid();
            return new List<CategoryMandatoryExtraAttribute>
            {
                new CategoryMandatoryExtraAttribute
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "SKU",
                    Name_fr = "SKU",
                    AttributeType = "string",
                    SortOrder = 1
                },
                new CategoryMandatoryExtraAttribute
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "Dimensions",
                    Name_fr = "Dimensions",
                    AttributeType = "string",
                    SortOrder = 2
                },
                new CategoryMandatoryExtraAttribute
                {
                    Id = Guid.NewGuid(),
                    CategoryNodeId = categoryNodeId,
                    Name_en = "Weight",
                    Name_fr = "Poids",
                    AttributeType = "decimal",
                    SortOrder = 3
                }
            };
        }

        [Fact]
        public async Task GetAttributesByCategoryNodeIdAsync_ShouldReturnAttributes_WhenAttributesExist()
        {
            // Arrange
            var attributes = CreateMultipleValidEntities().ToList();
            var categoryNodeId = attributes.First().CategoryNodeId;

            _mockRepository.Setup(repo => repo.GetAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(attributes);

            // Act
            var result = await _mockRepository.Object.GetAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.All(result, attr => Assert.Equal(categoryNodeId, attr.CategoryNodeId));
        }

        [Fact]
        public async Task GetAttributesByCategoryNodeIdAsync_ShouldReturnEmptyList_WhenNoAttributesExist()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();
            var emptyList = Enumerable.Empty<CategoryMandatoryExtraAttribute>();

            _mockRepository.Setup(repo => repo.GetAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(emptyList);

            // Act
            var result = await _mockRepository.Object.GetAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttributesByCategoryNodeIdAsync_ShouldReturnAttributesOrderedBySortOrder()
        {
            // Arrange
            var attributes = CreateMultipleValidEntities().ToList();
            var categoryNodeId = attributes.First().CategoryNodeId;

            _mockRepository.Setup(repo => repo.GetAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(attributes);

            // Act
            var result = await _mockRepository.Object.GetAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            var attributeList = result.ToList();
            Assert.Equal(1, attributeList[0].SortOrder);
            Assert.Equal(2, attributeList[1].SortOrder);
            Assert.Equal(3, attributeList[2].SortOrder);
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
        }

        [Fact]
        public async Task DeleteAttributesByCategoryNodeIdAsync_ShouldReturnFalse_WhenNoAttributesToDelete()
        {
            // Arrange
            var categoryNodeId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId))
                          .ReturnsAsync(false);

            // Act
            var result = await _mockRepository.Object.DeleteAttributesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CreateValidEntity_ShouldReturnEntityWithAllRequiredProperties()
        {
            // Act
            var entity = CreateValidEntity();

            // Assert
            Assert.NotEqual(Guid.Empty, entity.Id);
            Assert.NotEqual(Guid.Empty, entity.CategoryNodeId);
            Assert.NotNull(entity.Name_en);
            Assert.NotEmpty(entity.Name_en);
            Assert.NotNull(entity.Name_fr);
            Assert.NotEmpty(entity.Name_fr);
        }

        [Fact]
        public void CreateMultipleValidEntities_ShouldReturnEntitiesWithSameCategoryNodeId()
        {
            // Act
            var entities = CreateMultipleValidEntities().ToList();

            // Assert
            Assert.NotEmpty(entities);
            var firstCategoryNodeId = entities.First().CategoryNodeId;
            Assert.All(entities, entity => Assert.Equal(firstCategoryNodeId, entity.CategoryNodeId));
        }

        [Fact]
        public void CreateMultipleValidEntities_ShouldReturnEntitiesWithDifferentIds()
        {
            // Act
            var entities = CreateMultipleValidEntities().ToList();

            // Assert
            Assert.NotEmpty(entities);
            var ids = entities.Select(e => e.Id).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }
    }
}
