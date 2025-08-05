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
                Name = "Test Item",
                Description = "Test Description",
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>()
            };

            var item = new Item
            {
                Id = Guid.NewGuid(),
                SellerID = createItemRequest.SellerID,
                Name = createItemRequest.Name,
                Description = createItemRequest.Description,
                Brand = createItemRequest.Brand,
                Category = createItemRequest.Category,
                Variants = createItemRequest.Variants,
                ImageUrls = createItemRequest.ImageUrls,
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
            Assert.Equal(createItemRequest.Name, result.Value.Name);
            Assert.Equal(createItemRequest.SellerID, result.Value.SellerID);
        }

        [Fact]
        public async Task CreateItemAsync_ReturnFailure_WhenValidationFails()
        {
            // Arrange
            var createItemRequest = new CreateItemRequest
            {
                SellerID = Guid.Empty, // Invalid SellerID
                Name = "", // Invalid Name
                Description = "Test Description",
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>()
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
                    Name = "Test Item 1",
                    Description = "Test Description 1",
                    Brand = "Test Brand 1",
                    Category = "Test Category 1",
                    Variants = new List<ItemVariant>(),
                    ImageUrls = new List<string>(),
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
                Name = "Test Item",
                Description = "Test Description",
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>(),
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
                Name = "Updated Item",
                Description = "Updated Description",
                Brand = "Updated Brand",
                Category = "Updated Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>()
            };

            var existingItem = new Item
            {
                Id = updateItemRequest.Id,
                SellerID = Guid.NewGuid(),
                Name = "Original Item",
                Description = "Original Description",
                Brand = "Original Brand",
                Category = "Original Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>(),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null,
                Deleted = false
            };

            var updatedItem = new Item
            {
                Id = updateItemRequest.Id,
                SellerID = updateItemRequest.SellerID,
                Name = updateItemRequest.Name,
                Description = updateItemRequest.Description,
                Brand = updateItemRequest.Brand,
                Category = updateItemRequest.Category,
                Variants = updateItemRequest.Variants,
                ImageUrls = updateItemRequest.ImageUrls,
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
            Assert.Equal(updateItemRequest.Name, result.Value.Name);
        }

        [Fact]
        public async Task UpdateItemAsync_ReturnFailure_WhenItemDoesNotExist()
        {
            // Arrange
            var updateItemRequest = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name = "Updated Item",
                Description = "Updated Description",
                Brand = "Updated Brand",
                Category = "Updated Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>()
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
                Name = "Test Item",
                Description = "Test Description",
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>(),
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