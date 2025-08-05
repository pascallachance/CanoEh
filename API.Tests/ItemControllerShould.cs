using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests
{
    public class ItemControllerShould
    {
        private readonly Mock<IItemService> _mockItemService;
        private readonly ItemController _controller;

        public ItemControllerShould()
        {
            _mockItemService = new Mock<IItemService>();
            _controller = new ItemController(_mockItemService.Object);
        }

        [Fact]
        public async Task CreateItem_ReturnOk_WhenItemCreatedSuccessfully()
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

            var createItemResponse = new CreateItemResponse
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

            var result = Result.Success(createItemResponse);
            _mockItemService.Setup(x => x.CreateItemAsync(It.IsAny<CreateItemRequest>()))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.CreateItem(createItemRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateItem_ReturnBadRequest_WhenValidationFails()
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

            var result = Result.Failure<CreateItemResponse>("Validation failed.", StatusCodes.Status400BadRequest);
            _mockItemService.Setup(x => x.CreateItemAsync(It.IsAny<CreateItemRequest>()))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.CreateItem(createItemRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetAllItems_ReturnOk_WhenItemsExist()
        {
            // Arrange
            var items = new List<GetItemResponse>
            {
                new GetItemResponse
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

            var result = Result.Success<IEnumerable<GetItemResponse>>(items);
            _mockItemService.Setup(x => x.GetAllItemsAsync())
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.GetAllItems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetItemById_ReturnOk_WhenItemExists()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var getItemResponse = new GetItemResponse
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

            var result = Result.Success(getItemResponse);
            _mockItemService.Setup(x => x.GetItemByIdAsync(itemId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.GetItemById(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetItemById_ReturnNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var result = Result.Failure<GetItemResponse>("Item not found.", StatusCodes.Status404NotFound);
            _mockItemService.Setup(x => x.GetItemByIdAsync(itemId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.GetItemById(itemId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateItem_ReturnOk_WhenItemUpdatedSuccessfully()
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

            var updateItemResponse = new UpdateItemResponse
            {
                Id = updateItemRequest.Id,
                SellerID = updateItemRequest.SellerID,
                Name = updateItemRequest.Name,
                Description = updateItemRequest.Description,
                Brand = updateItemRequest.Brand,
                Category = updateItemRequest.Category,
                Variants = updateItemRequest.Variants,
                ImageUrls = updateItemRequest.ImageUrls,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                Deleted = false
            };

            var result = Result.Success(updateItemResponse);
            _mockItemService.Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>()))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.UpdateItem(updateItemRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateItem_ReturnNotFound_WhenItemDoesNotExist()
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

            var result = Result.Failure<UpdateItemResponse>("Item not found.", StatusCodes.Status404NotFound);
            _mockItemService.Setup(x => x.UpdateItemAsync(It.IsAny<UpdateItemRequest>()))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.UpdateItem(updateItemRequest);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_ReturnOk_WhenItemDeletedSuccessfully()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var deleteItemResponse = new DeleteItemResponse
            {
                Id = itemId,
                Message = "Item deleted successfully.",
                Success = true
            };

            var result = Result.Success(deleteItemResponse);
            _mockItemService.Setup(x => x.DeleteItemAsync(itemId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteItem(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteItem_ReturnNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var result = Result.Failure<DeleteItemResponse>("Item not found.", StatusCodes.Status404NotFound);
            _mockItemService.Setup(x => x.DeleteItemAsync(itemId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteItem(itemId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteItemVariant_ReturnOk_WhenVariantDeletedSuccessfully()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var deleteItemVariantResponse = new DeleteItemVariantResponse
            {
                ItemId = itemId,
                VariantId = variantId,
                Message = "Item variant deleted successfully.",
                Success = true
            };

            var result = Result.Success(deleteItemVariantResponse);
            _mockItemService.Setup(x => x.DeleteItemVariantAsync(itemId, variantId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteItemVariant(itemId, variantId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteItemVariant_ReturnNotFound_WhenItemOrVariantDoesNotExist()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var result = Result.Failure<DeleteItemVariantResponse>("Item or variant not found.", StatusCodes.Status404NotFound);
            _mockItemService.Setup(x => x.DeleteItemVariantAsync(itemId, variantId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteItemVariant(itemId, variantId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
    }
}