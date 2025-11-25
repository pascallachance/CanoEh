using Domain.Models.Requests;
using Domain.Services.Implementations;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class ItemServiceShould
    {
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly Mock<IItemVariantRepository> _mockItemVariantRepository;
        private readonly Mock<IItemAttributeRepository> _mockItemAttributeRepository;
        private readonly Mock<IItemVariantAttributeRepository> _mockItemVariantAttributeRepository;
        private readonly ItemService _itemService;

        public ItemServiceShould()
        {
            _mockItemRepository = new Mock<IItemRepository>();
            _mockItemVariantRepository = new Mock<IItemVariantRepository>();
            _mockItemAttributeRepository = new Mock<IItemAttributeRepository>();
            _mockItemVariantAttributeRepository = new Mock<IItemVariantAttributeRepository>();
            _itemService = new ItemService(
                _mockItemRepository.Object, 
                _mockItemVariantRepository.Object,
                _mockItemAttributeRepository.Object,
                _mockItemVariantAttributeRepository.Object,
                "Server=(localdb)\\MSSQLLocalDB;Database=CanoEh;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        [Fact]
        public async Task CreateItemAsync_ReturnSuccess_WhenValidRequest()
        {
            // Arrange
            var createItemRequest = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>(),
                ItemAttributes = new List<CreateItemAttributeRequest>()
            };

            // Act
            var result = await _itemService.CreateItemAsync(createItemRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(createItemRequest.Name_en, result.Value.Name_en);
            Assert.Equal(createItemRequest.Name_fr, result.Value.Name_fr);
            Assert.Equal(createItemRequest.SellerID, result.Value.SellerID);
        }

        [Fact]
        public async Task CreateItemAsync_ReturnFailure_WhenValidationFails()
        {
            // Arrange
            var createItemRequest = new CreateItemRequest
            {
                SellerID = Guid.Empty, // Invalid SellerID
                Name_en = "", // Invalid Name_en
                Name_fr = "", // Invalid Name_fr
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>(),
                ItemAttributes = new List<CreateItemAttributeRequest>()
            };

            // Act
            var result = await _itemService.CreateItemAsync(createItemRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task GetAllItemsAsync_ReturnSuccess_WhenItemsExist()
        {
            // Arrange
            var items = new List<Item>
            {
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Test Item 1",
                    Name_fr = "Article de test 1",
                    Description_en = "Test item 1 description EN",
                    Description_fr = "Test item 1 description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>(),
                    ItemAttributes = new List<ItemAttribute>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(x => x.GetAllAsync())
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetAllItemsAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
        }

        [Fact]
        public async Task GetItemByIdAsync_ReturnSuccess_WhenItemExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item
            {
                Id = itemId,
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test item description EN",
                Description_fr = "Test item description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            _mockItemRepository.Setup(x => x.GetItemByIdAsync(itemId))
                              .ReturnsAsync(item);

            // Act
            var result = await _itemService.GetItemByIdAsync(itemId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(itemId, result.Value.Id);
        }

        [Fact]
        public async Task GetItemByIdAsync_ReturnFailure_WhenItemDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockItemRepository.Setup(x => x.GetItemByIdAsync(itemId))
                              .ReturnsAsync((Item?)null);

            // Act
            var result = await _itemService.GetItemByIdAsync(itemId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Item not found.", result.Error);
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnSuccess_WhenValidRequest()
        {
            // Arrange
            var updateItemRequest = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Updated Item",
                Name_fr = "Article mis à jour",
                Description_en = "Updated Description EN",
                Description_fr = "Updated Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>()
            };

            var existingItem = new Item
            {
                Id = updateItemRequest.Id,
                SellerID = Guid.NewGuid(),
                Name_en = "Original Item",
                Name_fr = "Article original",
                Description_en = "Original Description EN",
                Description_fr = "Original Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>(),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null,
                Deleted = false
            };

            var updatedItem = new Item
            {
                Id = updateItemRequest.Id,
                SellerID = updateItemRequest.SellerID,
                Name_en = updateItemRequest.Name_en,
                Name_fr = updateItemRequest.Name_fr,
                Description_en = updateItemRequest.Description_en,
                Description_fr = updateItemRequest.Description_fr,
                CategoryID = updateItemRequest.CategoryID,
                Variants = updateItemRequest.Variants,
                ItemAttributes = updateItemRequest.ItemAttributes,
                CreatedAt = existingItem.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                Deleted = false
            };

            _mockItemRepository.Setup(x => x.GetItemByIdAsync(updateItemRequest.Id))
                              .ReturnsAsync(existingItem);
            _mockItemRepository.Setup(x => x.UpdateAsync(It.IsAny<Item>()))
                              .ReturnsAsync(updatedItem);

            // Act
            var result = await _itemService.UpdateItemAsync(updateItemRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(updateItemRequest.Name_en, result.Value.Name_en);
            Assert.Equal(updateItemRequest.Name_fr, result.Value.Name_fr);
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnFailure_WhenItemDoesNotExist()
        {
            // Arrange
            var updateItemRequest = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Updated Item",
                Name_fr = "Article mis à jour",
                Description_en = "Updated Description EN",
                Description_fr = "Updated Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>()
            };

            _mockItemRepository.Setup(x => x.GetItemByIdAsync(updateItemRequest.Id))
                              .ReturnsAsync((Item?)null);

            // Act
            var result = await _itemService.UpdateItemAsync(updateItemRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Item not found.", result.Error);
        }

        [Fact]
        public async Task DeleteItemAsync_ReturnSuccess_WhenItemExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new Item
            {
                Id = itemId,
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test item description EN",
                Description_fr = "Test item description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            _mockItemRepository.Setup(x => x.GetItemByIdAsync(itemId))
                              .ReturnsAsync(item);
            _mockItemRepository.Setup(x => x.DeleteAsync(It.IsAny<Item>()))
                              .Returns(Task.CompletedTask);

            // Act
            var result = await _itemService.DeleteItemAsync(itemId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(itemId, result.Value.Id);
            Assert.True(result.Value.Success);
        }

        [Fact]
        public async Task DeleteItemAsync_ReturnFailure_WhenItemDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockItemRepository.Setup(x => x.GetItemByIdAsync(itemId))
                              .ReturnsAsync((Item?)null);

            // Act
            var result = await _itemService.DeleteItemAsync(itemId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Item not found.", result.Error);
        }

        [Fact]
        public async Task DeleteItemVariantAsync_ReturnSuccess_WhenVariantExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            _mockItemVariantRepository.Setup(x => x.DeleteItemVariantAsync(itemId, variantId))
                              .ReturnsAsync(true);

            // Act
            var result = await _itemService.DeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(itemId, result.Value.ItemId);
            Assert.Equal(variantId, result.Value.VariantId);
            Assert.True(result.Value.Success);
        }

        [Fact]
        public async Task DeleteItemVariantAsync_ReturnFailure_WhenVariantDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            _mockItemVariantRepository.Setup(x => x.DeleteItemVariantAsync(itemId, variantId))
                              .ReturnsAsync(false);

            // Act
            var result = await _itemService.DeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Item or variant not found.", result.Error);
        }

        [Fact]
        public async Task CreateItemAsync_MapsVariantsCorrectly_WhenVariantsProvided()
        {
            // Arrange
            var createItemRequest = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>
                {
                    new CreateItemVariantRequest
                    {
                        Price = 19.99m,
                        StockQuantity = 100,
                        Sku = "TEST-SKU-001",
                        ProductIdentifierType = "UPC",
                        ProductIdentifierValue = "123456789012",
                        ItemVariantName_en = "Color: Red",
                        ItemVariantName_fr = "Couleur: Rouge",
                        Deleted = false,
                        ItemVariantAttributes = new List<CreateItemVariantAttributeRequest>()
                    }
                },
                ItemAttributes = new List<CreateItemAttributeRequest>()
            };

            // Act
            var result = await _itemService.CreateItemAsync(createItemRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Variants);
            Assert.Single(result.Value.Variants);
            var variant = result.Value.Variants.First();
            Assert.Equal(19.99m, variant.Price);
            Assert.Equal(100, variant.StockQuantity);
            Assert.Equal("TEST-SKU-001", variant.Sku);
            Assert.Equal("UPC", variant.ProductIdentifierType);
            Assert.Equal("123456789012", variant.ProductIdentifierValue);
            Assert.Equal("Color: Red", variant.ItemVariantName_en);
            Assert.Equal("Couleur: Rouge", variant.ItemVariantName_fr);
        }

        [Fact]
        public async Task CreateItemAsync_MapsItemAttributesCorrectly_WhenAttributesProvided()
        {
            // Arrange
            var createItemRequest = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>(),
                ItemAttributes = new List<CreateItemAttributeRequest>
                {
                    new CreateItemAttributeRequest
                    {
                        AttributeName_en = "Material",
                        AttributeName_fr = "Materiaux",
                        Attributes_en = "Cotton",
                        Attributes_fr = "Coton"
                    }
                }
            };

            // Act
            var result = await _itemService.CreateItemAsync(createItemRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.ItemAttributes);
            Assert.Single(result.Value.ItemAttributes);
            var attribute = result.Value.ItemAttributes.First();
            Assert.Equal("Material", attribute.AttributeName_en);
            Assert.Equal("Materiaux", attribute.AttributeName_fr);
            Assert.Equal("Cotton", attribute.Attributes_en);
            Assert.Equal("Coton", attribute.Attributes_fr);
        }

        [Fact]
        public async Task CreateItemAsync_MapsItemVariantAttributesCorrectly_WhenVariantAttributesProvided()
        {
            // Arrange
            var createItemRequest = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>
                {
                    new CreateItemVariantRequest
                    {
                        Price = 29.99m,
                        StockQuantity = 50,
                        Sku = "TEST-SKU-002",
                        ItemVariantName_en = "Variant 1",
                        ItemVariantName_fr = "Variante 1",
                        ItemVariantAttributes = new List<CreateItemVariantAttributeRequest>
                        {
                            new CreateItemVariantAttributeRequest
                            {
                                AttributeName_en = "Size",
                                AttributeName_fr = "Taille",
                                Attributes_en = "Large",
                                Attributes_fr = "Grand"
                            },
                            new CreateItemVariantAttributeRequest
                            {
                                AttributeName_en = "Color",
                                AttributeName_fr = "Couleur",
                                Attributes_en = "Blue",
                                Attributes_fr = "Bleu"
                            }
                        }
                    }
                },
                ItemAttributes = new List<CreateItemAttributeRequest>()
            };

            // Act
            var result = await _itemService.CreateItemAsync(createItemRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value.Variants);
            Assert.Single(result.Value.Variants);
            var variant = result.Value.Variants.First();
            Assert.NotNull(variant.ItemVariantAttributes);
            Assert.Equal(2, variant.ItemVariantAttributes.Count);
            
            var sizeAttribute = variant.ItemVariantAttributes.First();
            Assert.Equal("Size", sizeAttribute.AttributeName_en);
            Assert.Equal("Taille", sizeAttribute.AttributeName_fr);
            Assert.Equal("Large", sizeAttribute.Attributes_en);
            Assert.Equal("Grand", sizeAttribute.Attributes_fr);

            var colorAttribute = variant.ItemVariantAttributes.Last();
            Assert.Equal("Color", colorAttribute.AttributeName_en);
            Assert.Equal("Couleur", colorAttribute.AttributeName_fr);
            Assert.Equal("Blue", colorAttribute.Attributes_en);
            Assert.Equal("Bleu", colorAttribute.Attributes_fr);
        }

        // Note: The following tests document the expected behavior of transaction error handling.
        // In actual database failure scenarios during the transaction, the service will now return
        // specific error messages like:
        // - "Failed to insert Item: {details}" - when Item insertion fails
        // - "Failed to insert ItemAttributes: {details}" - when ItemAttributes insertion fails  
        // - "Failed to insert ItemVariant: {details}" - when ItemVariant insertion fails
        // - "Failed to insert ItemVariantAttributes: {details}" - when ItemVariantAttributes insertion fails
        //
        // These cannot be easily unit-tested without a real database or complex mocking of Dapper,
        // but the code changes ensure more helpful error messages for debugging production issues.

        [Fact]
        public async Task GetAllItemsFromSellerAsync_ReturnSuccess_WhenSellerHasItems()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var items = new List<Item>
            {
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = sellerId,
                    Name_en = "Test Item 1",
                    Name_fr = "Article de test 1",
                    Description_en = "Test item 1 description EN",
                    Description_fr = "Test item 1 description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>(),
                    ItemAttributes = new List<ItemAttribute>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                },
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = sellerId,
                    Name_en = "Test Item 2",
                    Name_fr = "Article de test 2",
                    Description_en = "Test item 2 description EN",
                    Description_fr = "Test item 2 description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>(),
                    ItemAttributes = new List<ItemAttribute>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(x => x.GetAllFromSellerByID(sellerId))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetAllItemsFromSellerAsync(sellerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());
            Assert.All(result.Value, item => Assert.Equal(sellerId, item.SellerID));
        }

        [Fact]
        public async Task GetAllItemsFromSellerAsync_ReturnEmptyList_WhenSellerHasNoItems()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var items = new List<Item>();

            _mockItemRepository.Setup(x => x.GetAllFromSellerByID(sellerId))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetAllItemsFromSellerAsync(sellerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetAllItemsFromSellerAsync_ReturnFailure_WhenRepositoryThrowsException()
        {
            // Arrange
            var sellerId = Guid.NewGuid();

            _mockItemRepository.Setup(x => x.GetAllFromSellerByID(sellerId))
                              .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _itemService.GetAllItemsFromSellerAsync(sellerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An error occurred while retrieving items for the seller", result.Error);
        }
    }
}