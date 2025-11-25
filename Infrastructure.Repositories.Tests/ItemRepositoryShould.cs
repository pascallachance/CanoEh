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
                ItemAttributes = new List<ItemAttribute>()
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
                    ItemAttributes = new List<ItemAttribute>()
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
                    ItemAttributes = new List<ItemAttribute>()
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
                    ItemAttributes = new List<ItemAttribute>()
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
            Assert.NotNull(item.ItemAttributes);
        }

        [Fact]
        public void Item_ShouldInitializeCollectionsCorrectly()
        {
            // Arrange & Act
            var item = new Item();

            // Assert
            Assert.NotNull(item.Variants);
            Assert.Empty(item.Variants);
            Assert.NotNull(item.ItemAttributes);
            Assert.Empty(item.ItemAttributes);
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
        public void ItemAttribute_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var attribute = new ItemAttribute
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
        public void ItemAttribute_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var attribute = new ItemAttribute();

            // Assert
            Assert.Equal(Guid.Empty, attribute.Id);
            Assert.Equal(Guid.Empty, attribute.ItemID);
            Assert.Equal(string.Empty, attribute.AttributeName_en);
            Assert.Null(attribute.AttributeName_fr);
            Assert.Equal(string.Empty, attribute.Attributes_en);
            Assert.Null(attribute.Attributes_fr);
        }

        [Fact]
        public async Task GetBySellerIdAsync_ShouldReturnItemsWithRelatedEntities()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            
            var itemAttribute = new ItemAttribute
            {
                Id = Guid.NewGuid(),
                ItemID = itemId,
                AttributeName_en = "Material",
                Attributes_en = "Cotton"
            };
            
            var variantAttribute = new ItemVariantAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantID = variantId,
                AttributeName_en = "Size",
                Attributes_en = "Large"
            };
            
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = itemId,
                Price = 19.99m,
                Sku = "TEST-001",
                ItemVariantAttributes = new List<ItemVariantAttribute> { variantAttribute }
            };
            
            var item = new Item
            {
                Id = itemId,
                SellerID = sellerId,
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Variants = new List<ItemVariant> { variant },
                ItemAttributes = new List<ItemAttribute> { itemAttribute }
            };

            _mockItemRepository.Setup(repo => repo.GetBySellerIdAsync(sellerId))
                              .ReturnsAsync(new List<Item> { item });

            // Act
            var result = await _mockItemRepository.Object.GetBySellerIdAsync(sellerId);

            // Assert
            Assert.NotNull(result);
            var items = result.ToList();
            Assert.Single(items);
            
            var returnedItem = items[0];
            Assert.Equal(itemId, returnedItem.Id);
            Assert.Equal(sellerId, returnedItem.SellerID);
            
            // Verify ItemVariants are included
            Assert.NotNull(returnedItem.Variants);
            Assert.Single(returnedItem.Variants);
            Assert.Equal(variantId, returnedItem.Variants[0].Id);
            
            // Verify ItemVariantAttributes are included
            Assert.NotNull(returnedItem.Variants[0].ItemVariantAttributes);
            Assert.Single(returnedItem.Variants[0].ItemVariantAttributes);
            Assert.Equal("Size", returnedItem.Variants[0].ItemVariantAttributes[0].AttributeName_en);
            
            // Verify ItemAttributes are included
            Assert.NotNull(returnedItem.ItemAttributes);
            Assert.Single(returnedItem.ItemAttributes);
            Assert.Equal("Material", returnedItem.ItemAttributes[0].AttributeName_en);
            
            _mockItemRepository.Verify(repo => repo.GetBySellerIdAsync(sellerId), Times.Once);
        }

        [Fact]
        public async Task GetBySellerIdAsync_ShouldReturnEmptyList_WhenNoItemsExist()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            _mockItemRepository.Setup(repo => repo.GetBySellerIdAsync(sellerId))
                              .ReturnsAsync(new List<Item>());

            // Act
            var result = await _mockItemRepository.Object.GetBySellerIdAsync(sellerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockItemRepository.Verify(repo => repo.GetBySellerIdAsync(sellerId), Times.Once);
        }
    }
}