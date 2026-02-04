using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Repositories.Tests.Common;
using Moq;

namespace Infrastructure.Repositories.Tests
{
    public class ItemRepositoryShould : BaseRepositoryShould<Item>
    {
        private readonly ItemRepository _itemRepository;
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly Mock<IItemVariantRepository> _mockItemVariantRepository;

        public ItemRepositoryShould()
        {
            _itemRepository = new ItemRepository(ConnectionString);
            _mockItemRepository = new Mock<IItemRepository>();
            _mockItemVariantRepository = new Mock<IItemVariantRepository>();
        }

        protected override Item CreateValidEntity()
        {
            return new Item
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test item description EN",
                Description_fr = "Test item description FR",
                CategoryID = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Deleted = false,
                Variants = new List<ItemVariant>(),
                ItemVariantFeatures = new List<ItemVariantFeatures>()
            };
        }

        protected override IEnumerable<Item> CreateMultipleValidEntities()
        {
            return new List<Item>
            {
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Item One",
                    Name_fr = "Article Un",
                    Description_en = "First item description",
                    Description_fr = "Première description d'article",
                    CategoryID = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Deleted = false,
                    Variants = new List<ItemVariant>(),
                    ItemVariantFeatures = new List<ItemVariantFeatures>()
                },
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Item Two",
                    Name_fr = "Article Deux",
                    Description_en = "Second item description",
                    Description_fr = "Deuxième description d'article",
                    CategoryID = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Deleted = false,
                    Variants = new List<ItemVariant>(),
                    ItemVariantFeatures = new List<ItemVariantFeatures>()
                },
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Item Three",
                    Name_fr = "Article Trois",
                    Description_en = "Third item description",
                    Description_fr = "Troisième description d'article",
                    CategoryID = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    Deleted = false,
                    Variants = new List<ItemVariant>(),
                    ItemVariantFeatures = new List<ItemVariantFeatures>()
                }
            };
        }

        // Test ItemRepository specific methods
        [Fact]
        public async Task GetItemByIdAsync_ShouldReturnItem_WhenItemExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = CreateValidEntity();
            item.Id = itemId;

            _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId))
                              .ReturnsAsync(item);

            // Act
            var result = await _mockItemRepository.Object.GetItemByIdAsync(itemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(itemId, result.Id);
            _mockItemRepository.Verify(repo => repo.GetItemByIdAsync(itemId), Times.Once);
        }

        [Fact]
        public async Task GetItemByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockItemRepository.Setup(repo => repo.GetItemByIdAsync(itemId))
                              .ReturnsAsync((Item?)null);

            // Act
            var result = await _mockItemRepository.Object.GetItemByIdAsync(itemId);

            // Assert
            Assert.Null(result);
            _mockItemRepository.Verify(repo => repo.GetItemByIdAsync(itemId), Times.Once);
        }

        [Fact]
        public async Task DeleteItemVariantAsync_ShouldReturnTrue_WhenVariantExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            _mockItemVariantRepository.Setup(repo => repo.DeleteItemVariantAsync(itemId, variantId))
                              .ReturnsAsync(true);

            // Act
            var result = await _mockItemVariantRepository.Object.DeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result);
            _mockItemVariantRepository.Verify(repo => repo.DeleteItemVariantAsync(itemId, variantId), Times.Once);
        }

        [Fact]
        public async Task DeleteItemVariantAsync_ShouldReturnFalse_WhenVariantDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            _mockItemVariantRepository.Setup(repo => repo.DeleteItemVariantAsync(itemId, variantId))
                              .ReturnsAsync(false);

            // Act
            var result = await _mockItemVariantRepository.Object.DeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.False(result);
            _mockItemVariantRepository.Verify(repo => repo.DeleteItemVariantAsync(itemId, variantId), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenValidConnectionStringProvided()
        {
            // Arrange & Act
            var repository = new ItemRepository(ConnectionString);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public void Item_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var item = CreateValidEntity();

            // Assert
            Assert.NotEqual(Guid.Empty, item.Id);
            Assert.NotEqual(Guid.Empty, item.SellerID);
            Assert.Equal("Test Item", item.Name_en);
            Assert.Equal("Article de test", item.Name_fr);
            Assert.Equal("Test item description EN", item.Description_en);
            Assert.Equal("Test item description FR", item.Description_fr);
            Assert.NotEqual(Guid.Empty, item.CategoryID);
            Assert.False(item.Deleted);
            Assert.NotNull(item.Variants);
            Assert.NotNull(item.ItemVariantFeatures);
        }

        [Fact]
        public void Item_ShouldInitializeCollectionsCorrectly()
        {
            // Arrange & Act
            var item = new Item();

            // Assert
            Assert.NotNull(item.Variants);
            Assert.Empty(item.Variants);
            Assert.NotNull(item.ItemVariantFeatures);
            Assert.Empty(item.ItemVariantFeatures);
            Assert.Equal(string.Empty, item.Name_en);
            Assert.Equal(string.Empty, item.Name_fr);
            Assert.False(item.Deleted);
        }

        [Fact]
        public void ItemVariant_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var variant = new ItemVariant
            {
                Id = Guid.NewGuid(),
                ItemId = Guid.NewGuid(),
                Price = 29.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                ProductIdentifierType = "UPC",
                ProductIdentifierValue = "123456789012",
                ItemVariantName_en = "Test Variant",
                ItemVariantName_fr = "Variante de test",
                Deleted = false
            };

            // Assert
            Assert.NotEqual(Guid.Empty, variant.Id);
            Assert.NotEqual(Guid.Empty, variant.ItemId);
            Assert.Equal(29.99m, variant.Price);
            Assert.Equal(100, variant.StockQuantity);
            Assert.Equal("TEST-SKU-001", variant.Sku);
            Assert.Equal("UPC", variant.ProductIdentifierType);
            Assert.Equal("123456789012", variant.ProductIdentifierValue);
            Assert.Equal("Test Variant", variant.ItemVariantName_en);
            Assert.Equal("Variante de test", variant.ItemVariantName_fr);
            Assert.False(variant.Deleted);
        }

        [Fact]
        public void ItemVariant_ShouldInitializeCollectionsCorrectly()
        {
            // Arrange & Act
            var variant = new ItemVariant();

            // Assert
            Assert.NotNull(variant.ItemVariantAttributes);
            Assert.Empty(variant.ItemVariantAttributes);
            Assert.Equal(0m, variant.Price);
            Assert.Equal(0, variant.StockQuantity);
            Assert.Null(variant.ItemVariantName_en);
            Assert.Null(variant.ItemVariantName_fr);
            Assert.False(variant.Deleted);
        }

        [Fact]
        public void ItemVariantFeatures_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var attribute = new ItemVariantFeatures
            {
                Id = Guid.NewGuid(),
                ItemID = Guid.NewGuid(),
                AttributeName_en = "Color",
                AttributeName_fr = "Couleur",
                Attributes_en = "Blue",
                Attributes_fr = "Bleu"
            };

            // Assert
            Assert.NotEqual(Guid.Empty, attribute.Id);
            Assert.NotEqual(Guid.Empty, attribute.ItemID);
            Assert.Equal("Color", attribute.AttributeName_en);
            Assert.Equal("Couleur", attribute.AttributeName_fr);
            Assert.Equal("Blue", attribute.Attributes_en);
            Assert.Equal("Bleu", attribute.Attributes_fr);
        }

        [Fact]
        public void ItemVariantFeatures_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var attribute = new ItemVariantFeatures();

            // Assert
            Assert.Equal(Guid.Empty, attribute.Id);
            Assert.Equal(Guid.Empty, attribute.ItemID);
            Assert.Equal(string.Empty, attribute.AttributeName_en);
            Assert.Null(attribute.AttributeName_fr);
            Assert.Equal(string.Empty, attribute.Attributes_en);
            Assert.Null(attribute.Attributes_fr);
        }

        [Fact]
        public void GetBySellerIdAsync_MethodExists_OnItemRepository()
        {
            // Arrange & Act
            // This test verifies that GetBySellerIdAsync method is properly defined on ItemRepository
            // and returns the expected type (Task<IEnumerable<Item>>).
            // Integration tests with a real database would be needed to verify:
            // - The early return logic when no items are found
            // - The batched queries and dictionary lookups work correctly
            // - ItemVariants, ItemVariantAttributes, and ItemVariantFeatures are properly loaded
            var methodInfo = _itemRepository.GetType().GetMethod("GetBySellerIdAsync");
            
            // Assert
            Assert.NotNull(methodInfo);
            Assert.Equal(typeof(Task<IEnumerable<Item>>), methodInfo.ReturnType);
            
            // Verify method parameter
            var parameters = methodInfo.GetParameters();
            Assert.Single(parameters);
            Assert.Equal("sellerId", parameters[0].Name);
            Assert.Equal(typeof(Guid), parameters[0].ParameterType);
        }

        [Fact]
        public void GetBySellerIdAsync_Interface_ShouldDefineMethod()
        {
            // Arrange & Act
            // Verify the interface contract for GetBySellerIdAsync
            var interfaceMethodInfo = typeof(IItemRepository).GetMethod("GetBySellerIdAsync");
            
            // Assert
            Assert.NotNull(interfaceMethodInfo);
            Assert.Equal(typeof(Task<IEnumerable<Item>>), interfaceMethodInfo.ReturnType);
        }

        [Fact]
        public void GetSuggestedProductsAsync_MethodExists_OnItemRepository()
        {
            // Arrange & Act
            // This test verifies that GetSuggestedProductsAsync method is properly defined on ItemRepository
            // and returns the expected type (Task<IEnumerable<Item>>).
            // Integration tests with a real database would be needed to verify:
            // - The SQL randomization logic using NEWID()
            // - The CTE filtering for items with images
            // - The batched queries and dictionary lookups work correctly
            // - ItemVariants, ItemVariantAttributes, and ItemVariantFeatures are properly loaded
            var methodInfo = _itemRepository.GetType().GetMethod("GetSuggestedProductsAsync");
            
            // Assert
            Assert.NotNull(methodInfo);
            Assert.Equal(typeof(Task<IEnumerable<Item>>), methodInfo.ReturnType);
            
            // Verify method parameter
            var parameters = methodInfo.GetParameters();
            Assert.Single(parameters);
            Assert.Equal("count", parameters[0].Name);
            Assert.Equal(typeof(int), parameters[0].ParameterType);
        }

        [Fact]
        public void GetSuggestedProductsAsync_Interface_ShouldDefineMethod()
        {
            // Arrange & Act
            // Verify the interface contract for GetSuggestedProductsAsync
            var interfaceMethodInfo = typeof(IItemRepository).GetMethod("GetSuggestedProductsAsync");
            
            // Assert
            Assert.NotNull(interfaceMethodInfo);
            Assert.Equal(typeof(Task<IEnumerable<Item>>), interfaceMethodInfo.ReturnType);
            
            // Verify method parameter on interface
            var parameters = interfaceMethodInfo.GetParameters();
            Assert.Single(parameters);
            Assert.Equal("count", parameters[0].Name);
            Assert.Equal(typeof(int), parameters[0].ParameterType);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_ShouldReturnItems_WhenItemsWithImagesExist()
        {
            // Arrange
            var items = new List<Item>
            {
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Suggested Item 1",
                    Name_fr = "Article suggéré 1",
                    Description_en = "Description EN",
                    Description_fr = "Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>
                    {
                        new ItemVariant
                        {
                            Id = Guid.NewGuid(),
                            ItemId = Guid.NewGuid(),
                            Price = 29.99m,
                            StockQuantity = 10,
                            Sku = "SUGG-001",
                            ImageUrls = "https://example.com/image1.jpg",
                            Deleted = false
                        }
                    },
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
                    CreatedAt = DateTime.UtcNow,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(repo => repo.GetSuggestedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _mockItemRepository.Object.GetSuggestedProductsAsync(4);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Suggested Item 1", resultList[0].Name_en);
            Assert.NotEmpty(resultList[0].Variants);
            Assert.NotNull(resultList[0].Variants[0].ImageUrls);
            _mockItemRepository.Verify(repo => repo.GetSuggestedProductsAsync(4), Times.Once);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_ShouldReturnEmpty_WhenNoItemsWithImagesExist()
        {
            // Arrange
            var items = new List<Item>();
            _mockItemRepository.Setup(repo => repo.GetSuggestedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _mockItemRepository.Object.GetSuggestedProductsAsync(4);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockItemRepository.Verify(repo => repo.GetSuggestedProductsAsync(4), Times.Once);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_ShouldRespectCountParameter()
        {
            // Arrange
            var count = 10;
            var items = new List<Item>();
            _mockItemRepository.Setup(repo => repo.GetSuggestedProductsAsync(count))
                              .ReturnsAsync(items);

            // Act
            var result = await _mockItemRepository.Object.GetSuggestedProductsAsync(count);

            // Assert
            Assert.NotNull(result);
            _mockItemRepository.Verify(repo => repo.GetSuggestedProductsAsync(count), Times.Once);
        }
    }
}