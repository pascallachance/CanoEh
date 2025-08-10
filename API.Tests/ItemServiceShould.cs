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
        private readonly ItemService _itemService;

        public ItemServiceShould()
        {
            _mockItemRepository = new Mock<IItemRepository>();
            _itemService = new ItemService(_mockItemRepository.Object);
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
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>()
            };

            var item = new Item
            {
                Id = Guid.NewGuid(),
                SellerID = createItemRequest.SellerID,
                Name_en = createItemRequest.Name_en,
                Name_fr = createItemRequest.Name_fr,
                Description_en = createItemRequest.Description_en,
                Description_fr = createItemRequest.Description_fr,
                CategoryID = createItemRequest.CategoryID,
                Variants = createItemRequest.Variants,
                ItemAttributes = createItemRequest.ItemAttributes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            _mockItemRepository.Setup(x => x.AddAsync(It.IsAny<Item>()))
                              .ReturnsAsync(item);

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
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>()
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

            _mockItemRepository.Setup(x => x.DeleteItemVariantAsync(itemId, variantId))
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

            _mockItemRepository.Setup(x => x.DeleteItemVariantAsync(itemId, variantId))
                              .ReturnsAsync(false);

            // Act
            var result = await _itemService.DeleteItemVariantAsync(itemId, variantId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            Assert.Equal("Item or variant not found.", result.Error);
        }
    }
}