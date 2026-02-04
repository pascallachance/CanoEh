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
        private readonly Mock<IItemVariantFeaturesRepository> _mockItemVariantFeaturesRepository;
        private readonly Mock<IItemVariantAttributeRepository> _mockItemVariantAttributeRepository;
        private readonly ItemService _itemService;

        public ItemServiceShould()
        {
            _mockItemRepository = new Mock<IItemRepository>();
            _mockItemVariantRepository = new Mock<IItemVariantRepository>();
            _mockItemVariantFeaturesRepository = new Mock<IItemVariantFeaturesRepository>();
            _mockItemVariantAttributeRepository = new Mock<IItemVariantAttributeRepository>();
            _itemService = new ItemService(
                _mockItemRepository.Object, 
                _mockItemVariantRepository.Object,
                _mockItemVariantFeaturesRepository.Object,
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
                ItemVariantFeatures = new List<CreateItemVariantFeaturesRequest>()
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
                ItemVariantFeatures = new List<CreateItemVariantFeaturesRequest>()
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
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
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
                ItemVariantFeatures = new List<ItemVariantFeatures>(),
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
                ItemVariantFeatures = new List<ItemVariantFeatures>()
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
                ItemVariantFeatures = new List<ItemVariantFeatures>(),
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
                ItemVariantFeatures = updateItemRequest.ItemVariantFeatures,
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
                ItemVariantFeatures = new List<ItemVariantFeatures>()
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
                ItemVariantFeatures = new List<ItemVariantFeatures>(),
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
                ItemVariantFeatures = new List<CreateItemVariantFeaturesRequest>()
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
        public async Task CreateItemAsync_MapsItemVariantFeaturesCorrectly_WhenAttributesProvided()
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
                ItemVariantFeatures = new List<CreateItemVariantFeaturesRequest>
                {
                    new CreateItemVariantFeaturesRequest
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
            Assert.NotNull(result.Value.ItemVariantFeatures);
            Assert.Single(result.Value.ItemVariantFeatures);
            var attribute = result.Value.ItemVariantFeatures.First();
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
                ItemVariantFeatures = new List<CreateItemVariantFeaturesRequest>()
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
        // - "Failed to insert ItemVariantFeatures: {details}" - when ItemVariantFeatures insertion fails  
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
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
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
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(x => x.GetBySellerIdAsync(sellerId, It.IsAny<bool>()))
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

            _mockItemRepository.Setup(x => x.GetBySellerIdAsync(sellerId, It.IsAny<bool>()))
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

            _mockItemRepository.Setup(x => x.GetBySellerIdAsync(sellerId, It.IsAny<bool>()))
                              .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _itemService.GetAllItemsFromSellerAsync(sellerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An error occurred while retrieving items for the seller", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantAsync_ReturnSuccess_WhenValidVariant()
        {
            // Arrange
            var variant = new ItemVariant
            {
                Id = Guid.NewGuid(),
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                ThumbnailUrl = "/uploads/test/test_thumb.jpg"
            };

            _mockItemVariantRepository.Setup(x => x.UpdateAsync(It.IsAny<ItemVariant>()))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UpdateItemVariantAsync(variant);

            // Assert
            Assert.True(result.IsSuccess);
            _mockItemVariantRepository.Verify(x => x.UpdateAsync(variant), Times.Once);
        }

        [Fact]
        public async Task UpdateItemVariantAsync_ReturnFailure_WhenVariantIsNull()
        {
            // Act
            var result = await _itemService.UpdateItemVariantAsync(null!);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid variant data.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantAsync_ReturnFailure_WhenVariantIdIsEmpty()
        {
            // Arrange
            var variant = new ItemVariant
            {
                Id = Guid.Empty,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001"
            };

            // Act
            var result = await _itemService.UpdateItemVariantAsync(variant);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid variant data.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnSuccess_WhenUpdatingThumbnail()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var imageUrl = "/uploads/company123/variant456/variant456_thumb.jpg";
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                ThumbnailUrl = null
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);
            _mockItemVariantRepository.Setup(x => x.UpdateAsync(It.IsAny<ItemVariant>()))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "thumbnail", imageUrl, 1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(imageUrl, variant.ThumbnailUrl);
            _mockItemVariantRepository.Verify(x => x.UpdateAsync(It.Is<ItemVariant>(v => v.ThumbnailUrl == imageUrl)), Times.Once);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnSuccess_WhenUpdatingProductImage()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var imageUrl = "/uploads/company123/variant456/variant456_1.jpg";
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                ImageUrls = null
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);
            _mockItemVariantRepository.Setup(x => x.UpdateAsync(It.IsAny<ItemVariant>()))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "image", imageUrl, 1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(imageUrl, variant.ImageUrls);
            _mockItemVariantRepository.Verify(x => x.UpdateAsync(It.Is<ItemVariant>(v => v.ImageUrls == imageUrl)), Times.Once);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_PreservesPositions_WhenUpdatingImageAtSpecificIndex()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var imageUrl3 = "/uploads/company123/variant456/variant456_3.jpg";
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                ImageUrls = null // Starting with no images
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);
            _mockItemVariantRepository.Setup(x => x.UpdateAsync(It.IsAny<ItemVariant>()))
                                     .ReturnsAsync(variant);

            // Act - Add image at position 3
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "image", imageUrl3, 3);

            // Assert
            Assert.True(result.IsSuccess);
            // Should have empty slots for positions 1 and 2, then the image at position 3
            var urls = variant.ImageUrls?.Split(',');
            Assert.NotNull(urls);
            Assert.Equal(3, urls.Length);
            Assert.Equal(string.Empty, urls[0]); // Position 1 (index 0)
            Assert.Equal(string.Empty, urls[1]); // Position 2 (index 1)
            Assert.Equal(imageUrl3, urls[2]);    // Position 3 (index 2)
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReplacesExistingImage_WhenUpdatingAtSamePosition()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var oldImageUrl = "/uploads/company123/variant456/variant456_2_old.jpg";
            var newImageUrl = "/uploads/company123/variant456/variant456_2_new.jpg";
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                ImageUrls = $",{oldImageUrl}," // Image at position 2
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);
            _mockItemVariantRepository.Setup(x => x.UpdateAsync(It.IsAny<ItemVariant>()))
                                     .ReturnsAsync(variant);

            // Act - Replace image at position 2
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "image", newImageUrl, 2);

            // Assert
            Assert.True(result.IsSuccess);
            var urls = variant.ImageUrls?.Split(',');
            Assert.NotNull(urls);
            Assert.Equal(3, urls.Length);
            Assert.Equal(newImageUrl, urls[1]); // Position 2 (index 1) should be updated
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenVariantIdIsEmpty()
        {
            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(Guid.Empty, "thumbnail", "/test.jpg", 1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid variant ID.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenImageUrlIsEmpty()
        {
            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(Guid.NewGuid(), "thumbnail", "", 1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Image URL is required.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenImageUrlIsNull()
        {
            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(Guid.NewGuid(), "thumbnail", null!, 1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Image URL is required.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenVariantNotFound()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync((ItemVariant?)null);

            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "thumbnail", "/test.jpg", 1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Variant not found.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenInvalidImageType()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001"
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "invalid", "/test.jpg", 1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Invalid image type. Must be 'thumbnail' or 'image'.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenImageNumberIsZero()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001"
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "image", "/test.jpg", 0);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Image number must be greater than 0.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenImageNumberIsNegative()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001"
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "image", "/test.jpg", -1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Image number must be greater than 0.", result.Error);
        }

        [Fact]
        public async Task UpdateItemVariantImageAsync_ReturnFailure_WhenRepositoryThrowsException()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = Guid.NewGuid(),
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001"
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);
            _mockItemVariantRepository.Setup(x => x.UpdateAsync(It.IsAny<ItemVariant>()))
                                     .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _itemService.UpdateItemVariantImageAsync(variantId, "thumbnail", "/test.jpg", 1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An error occurred while updating the variant image", result.Error);
        }

        [Fact]
        public async Task UnDeleteItemAsync_ReturnSuccess_WhenItemIsDeleted()
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
                ItemVariantFeatures = new List<ItemVariantFeatures>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = true // Item is deleted
            };

            _mockItemRepository.Setup(x => x.GetByIdAsync(itemId))
                              .ReturnsAsync(item);
            _mockItemRepository.Setup(x => x.UpdateAsync(It.IsAny<Item>()))
                              .ReturnsAsync(item);

            // Act
            var result = await _itemService.UnDeleteItemAsync(itemId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(itemId, result.Value.Id);
            Assert.True(result.Value.Success);
            Assert.Equal("Item undeleted successfully.", result.Value.Message);
        }

        [Fact]
        public async Task UnDeleteItemAsync_ReturnFailure_WhenItemDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            _mockItemRepository.Setup(x => x.GetByIdAsync(itemId))
                              .ThrowsAsync(new InvalidOperationException($"Item with id {itemId} not found"));

            // Act
            var result = await _itemService.UnDeleteItemAsync(itemId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An error occurred while undeleting the item", result.Error);
        }

        [Fact]
        public async Task UnDeleteItemAsync_ReturnFailure_WhenItemIsNotDeleted()
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
                ItemVariantFeatures = new List<ItemVariantFeatures>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false // Item is not deleted
            };

            _mockItemRepository.Setup(x => x.GetByIdAsync(itemId))
                              .ReturnsAsync(item);

            // Act
            var result = await _itemService.UnDeleteItemAsync(itemId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Item is not deleted.", result.Error);
        }

        [Fact]
        public async Task UnDeleteItemVariantAsync_ReturnSuccess_WhenVariantIsDeleted()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = itemId,
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                Deleted = true // Variant is deleted
            };
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
                ItemVariantFeatures = new List<ItemVariantFeatures>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);
            _mockItemVariantRepository.Setup(x => x.UpdateAsync(It.IsAny<ItemVariant>()))
                                     .ReturnsAsync(variant);
            _mockItemRepository.Setup(x => x.GetByIdAsync(itemId))
                              .ReturnsAsync(item);
            _mockItemRepository.Setup(x => x.UpdateAsync(It.IsAny<Item>()))
                              .ReturnsAsync(item);

            // Act
            var result = await _itemService.UnDeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(itemId, result.Value.ItemId);
            Assert.Equal(variantId, result.Value.VariantId);
            Assert.True(result.Value.Success);
            Assert.Equal("Item variant undeleted successfully.", result.Value.Message);
        }

        [Fact]
        public async Task UnDeleteItemVariantAsync_ReturnFailure_WhenVariantDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ThrowsAsync(new InvalidOperationException($"ItemVariant with id {variantId} not found"));

            // Act
            var result = await _itemService.UnDeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An error occurred while undeleting the item variant", result.Error);
        }

        [Fact]
        public async Task UnDeleteItemVariantAsync_ReturnFailure_WhenVariantIsNotDeleted()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = itemId,
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                Deleted = false // Variant is not deleted
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UnDeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Equal("Variant is not deleted.", result.Error);
        }

        [Fact]
        public async Task UnDeleteItemVariantAsync_ReturnFailure_WhenVariantBelongsToDifferentItem()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var differentItemId = Guid.NewGuid();
            var variant = new ItemVariant
            {
                Id = variantId,
                ItemId = differentItemId, // Different item ID
                Price = 19.99m,
                StockQuantity = 100,
                Sku = "TEST-SKU-001",
                Deleted = true
            };

            _mockItemVariantRepository.Setup(x => x.GetByIdAsync(variantId))
                                     .ReturnsAsync(variant);

            // Act
            var result = await _itemService.UnDeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Item or variant not found.", result.Error);
        }

        [Fact]
        public async Task GetRecentlyAddedProductsAsync_ReturnSuccess_WhenItemsExist()
        {
            // Arrange
            var items = new List<Item>
            {
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Recent Item 1",
                    Name_fr = "Article récent 1",
                    Description_en = "Test Description EN",
                    Description_fr = "Test Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>
                    {
                        new ItemVariant
                        {
                            Id = Guid.NewGuid(),
                            ItemId = Guid.NewGuid(),
                            Price = 29.99m,
                            StockQuantity = 10,
                            Sku = "TEST-001",
                            ImageUrls = "https://example.com/image1.jpg",
                            ThumbnailUrl = "https://example.com/thumb1.jpg",
                            ItemVariantAttributes = new List<ItemVariantAttribute>(),
                            Deleted = false
                        }
                    },
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                },
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Recent Item 2",
                    Name_fr = "Article récent 2",
                    Description_en = "Test Description EN",
                    Description_fr = "Test Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>(),
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(x => x.GetRecentlyAddedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetRecentlyAddedProductsAsync(4);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            var resultList = result.Value.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal("Recent Item 1", resultList[0].Name_en);
            Assert.Equal("Recent Item 2", resultList[1].Name_en);
        }

        [Fact]
        public async Task GetRecentlyAddedProductsAsync_ReturnSuccess_WhenNoItemsExist()
        {
            // Arrange
            var items = new List<Item>();
            _mockItemRepository.Setup(x => x.GetRecentlyAddedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetRecentlyAddedProductsAsync(100);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetRecentlyAddedProductsAsync_PassCorrectCountToRepository()
        {
            // Arrange
            var items = new List<Item>();
            _mockItemRepository.Setup(x => x.GetRecentlyAddedProductsAsync(50))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetRecentlyAddedProductsAsync(50);

            // Assert
            Assert.True(result.IsSuccess);
            _mockItemRepository.Verify(x => x.GetRecentlyAddedProductsAsync(50), Times.Once);
        }

        [Fact]
        public async Task GetRecentlyAddedProductsAsync_ReturnFailure_WhenExceptionOccurs()
        {
            // Arrange
            _mockItemRepository.Setup(x => x.GetRecentlyAddedProductsAsync(It.IsAny<int>()))
                              .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _itemService.GetRecentlyAddedProductsAsync(100);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An error occurred while retrieving recently added products", result.Error);
        }

        [Fact]
        public async Task GetRecentlyAddedProductsAsync_MapItemsToDtosCorrectly()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var attributeId = Guid.NewGuid();
            var variantAttributeId = Guid.NewGuid();

            var items = new List<Item>
            {
                new Item
                {
                    Id = itemId,
                    SellerID = Guid.NewGuid(),
                    Name_en = "Test Item",
                    Name_fr = "Article de test",
                    Description_en = "Description EN",
                    Description_fr = "Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>
                    {
                        new ItemVariant
                        {
                            Id = variantId,
                            ItemId = itemId,
                            Price = 19.99m,
                            StockQuantity = 5,
                            Sku = "SKU-001",
                            ImageUrls = "image1.jpg,image2.jpg",
                            ThumbnailUrl = "thumb.jpg",
                            ItemVariantName_en = "Variant EN",
                            ItemVariantName_fr = "Variant FR",
                            ItemVariantAttributes = new List<ItemVariantAttribute>
                            {
                                new ItemVariantAttribute
                                {
                                    Id = variantAttributeId,
                                    ItemVariantID = variantId,
                                    AttributeName_en = "Color",
                                    AttributeName_fr = "Couleur",
                                    Attributes_en = "Red",
                                    Attributes_fr = "Rouge"
                                }
                            },
                            Deleted = false
                        }
                    },
                    ItemVariantFeatures = new List<ItemVariantFeatures>
                    {
                        new ItemVariantFeatures
                        {
                            Id = attributeId,
                            ItemID = itemId,
                            AttributeName_en = "Material",
                            AttributeName_fr = "Matériau",
                            Attributes_en = "Cotton",
                            Attributes_fr = "Coton"
                        }
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(x => x.GetRecentlyAddedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetRecentlyAddedProductsAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            var item = result.Value.First();
            Assert.Equal(itemId, item.Id);
            Assert.Equal("Test Item", item.Name_en);
            Assert.Single(item.Variants);
            Assert.Single(item.ItemVariantFeatures);
            Assert.Equal(variantId, item.Variants[0].Id);
            Assert.Equal(19.99m, item.Variants[0].Price);
            Assert.Single(item.Variants[0].ItemVariantAttributes);
            Assert.Equal("Color", item.Variants[0].ItemVariantAttributes[0].AttributeName_en);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_ReturnSuccess_WhenItemsExist()
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
                    Description_en = "Test Description EN",
                    Description_fr = "Test Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>
                    {
                        new ItemVariant
                        {
                            Id = Guid.NewGuid(),
                            ItemId = Guid.NewGuid(),
                            Price = 39.99m,
                            StockQuantity = 15,
                            Sku = "SUGG-001",
                            ImageUrls = "https://example.com/suggested1.jpg",
                            ThumbnailUrl = "https://example.com/suggested_thumb1.jpg",
                            ItemVariantAttributes = new List<ItemVariantAttribute>(),
                            Deleted = false
                        }
                    },
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                },
                new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = Guid.NewGuid(),
                    Name_en = "Suggested Item 2",
                    Name_fr = "Article suggéré 2",
                    Description_en = "Test Description EN",
                    Description_fr = "Test Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>
                    {
                        new ItemVariant
                        {
                            Id = Guid.NewGuid(),
                            ItemId = Guid.NewGuid(),
                            Price = 49.99m,
                            StockQuantity = 8,
                            Sku = "SUGG-002",
                            ImageUrls = "https://example.com/suggested2.jpg",
                            ThumbnailUrl = "https://example.com/suggested_thumb2.jpg",
                            ItemVariantAttributes = new List<ItemVariantAttribute>(),
                            Deleted = false
                        }
                    },
                    ItemVariantFeatures = new List<ItemVariantFeatures>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(x => x.GetSuggestedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetSuggestedProductsAsync(4);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            var resultList = result.Value.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal("Suggested Item 1", resultList[0].Name_en);
            Assert.Equal("Suggested Item 2", resultList[1].Name_en);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_ReturnSuccess_WhenNoItemsExist()
        {
            // Arrange
            var items = new List<Item>();
            _mockItemRepository.Setup(x => x.GetSuggestedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetSuggestedProductsAsync(4);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_PassCorrectCountToRepository()
        {
            // Arrange
            var items = new List<Item>();
            _mockItemRepository.Setup(x => x.GetSuggestedProductsAsync(10))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetSuggestedProductsAsync(10);

            // Assert
            Assert.True(result.IsSuccess);
            _mockItemRepository.Verify(x => x.GetSuggestedProductsAsync(10), Times.Once);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_ReturnFailure_WhenExceptionOccurs()
        {
            // Arrange
            _mockItemRepository.Setup(x => x.GetSuggestedProductsAsync(It.IsAny<int>()))
                              .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _itemService.GetSuggestedProductsAsync(4);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.ErrorCode);
            Assert.Contains("An error occurred while retrieving suggested products", result.Error);
        }

        [Fact]
        public async Task GetSuggestedProductsAsync_MapItemsToDtosCorrectly()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var attributeId = Guid.NewGuid();
            var variantAttributeId = Guid.NewGuid();

            var items = new List<Item>
            {
                new Item
                {
                    Id = itemId,
                    SellerID = Guid.NewGuid(),
                    Name_en = "Suggested Test Item",
                    Name_fr = "Article de test suggéré",
                    Description_en = "Suggested Description EN",
                    Description_fr = "Suggested Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariant>
                    {
                        new ItemVariant
                        {
                            Id = variantId,
                            ItemId = itemId,
                            Price = 29.99m,
                            StockQuantity = 10,
                            Sku = "SUGG-SKU-001",
                            ImageUrls = "suggested_image1.jpg,suggested_image2.jpg",
                            ThumbnailUrl = "suggested_thumb.jpg",
                            ItemVariantName_en = "Suggested Variant EN",
                            ItemVariantName_fr = "Suggested Variant FR",
                            ItemVariantAttributes = new List<ItemVariantAttribute>
                            {
                                new ItemVariantAttribute
                                {
                                    Id = variantAttributeId,
                                    ItemVariantID = variantId,
                                    AttributeName_en = "Size",
                                    AttributeName_fr = "Taille",
                                    Attributes_en = "Large",
                                    Attributes_fr = "Grand"
                                }
                            },
                            Deleted = false
                        }
                    },
                    ItemVariantFeatures = new List<ItemVariantFeatures>
                    {
                        new ItemVariantFeatures
                        {
                            Id = attributeId,
                            ItemID = itemId,
                            AttributeName_en = "Brand",
                            AttributeName_fr = "Marque",
                            Attributes_en = "TestBrand",
                            Attributes_fr = "MarqueTest"
                        }
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            _mockItemRepository.Setup(x => x.GetSuggestedProductsAsync(It.IsAny<int>()))
                              .ReturnsAsync(items);

            // Act
            var result = await _itemService.GetSuggestedProductsAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            var item = result.Value.First();
            Assert.Equal(itemId, item.Id);
            Assert.Equal("Suggested Test Item", item.Name_en);
            Assert.Single(item.Variants);
            Assert.Single(item.ItemVariantFeatures);
            Assert.Equal(variantId, item.Variants[0].Id);
            Assert.Equal(29.99m, item.Variants[0].Price);
            Assert.Single(item.Variants[0].ItemVariantAttributes);
            Assert.Equal("Size", item.Variants[0].ItemVariantAttributes[0].AttributeName_en);
        }
    }
}