using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests
{
    public class ItemControllerShould
    {
        private readonly Mock<IItemService> _mockItemService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly Mock<ILogger<ItemController>> _mockLogger;
        private readonly ItemController _controller;

        public ItemControllerShould()
        {
            _mockItemService = new Mock<IItemService>();
            _mockUserService = new Mock<IUserService>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _mockLogger = new Mock<ILogger<ItemController>>();
            _controller = new ItemController(_mockItemService.Object, _mockUserService.Object, _mockFileStorageService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateItem_ReturnOk_WhenItemCreatedSuccessfully()
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

            var createItemResponse = new CreateItemResponse
            {
                Id = Guid.NewGuid(),
                SellerID = createItemRequest.SellerID,
                Name_en = createItemRequest.Name_en,
                Name_fr = createItemRequest.Name_fr,
                Description_en = createItemRequest.Description_en,
                Description_fr = createItemRequest.Description_fr,
                CategoryID = createItemRequest.CategoryID,
                Variants = new List<ItemVariantDto>(),
                ItemAttributes = new List<ItemAttributeDto>(),
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
                Name_en = "", // Invalid Name_en
                Name_fr = "", // Invalid Name_fr
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>(),
                ItemAttributes = new List<CreateItemAttributeRequest>()
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
                    Name_en = "Test Item 1",
                    Name_fr = "Article de test 1",
                    Description_en = "Test Description EN",
                    Description_fr = "Test Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariantDto>(),
                    ItemAttributes = new List<ItemAttributeDto>(),
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
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariantDto>(),
                ItemAttributes = new List<ItemAttributeDto>(),
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
                Name_en = "Updated Item",
                Name_fr = "Article mis à jour",
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>()
            };

            var updateItemResponse = new UpdateItemResponse
            {
                Id = updateItemRequest.Id,
                SellerID = updateItemRequest.SellerID,
                Name_en = updateItemRequest.Name_en,
                Name_fr = updateItemRequest.Name_fr,
                Description_en = updateItemRequest.Description_en,
                Description_fr = updateItemRequest.Description_fr,
                CategoryID = updateItemRequest.CategoryID,
                Variants = new List<ItemVariantDto>(),
                ItemAttributes = new List<ItemAttributeDto>(),
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
                Name_en = "Updated Item",
                Name_fr = "Article mis à jour",
                Description_en = "Test Description EN",
                Description_fr = "Test Description FR",
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>()
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

        [Fact]
        public async Task GetSellerItems_ReturnOk_WhenSellerHasItems()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var items = new List<GetItemResponse>
            {
                new GetItemResponse
                {
                    Id = Guid.NewGuid(),
                    SellerID = sellerId,
                    Name_en = "Test Item 1",
                    Name_fr = "Article de test 1",
                    Description_en = "Test Description EN",
                    Description_fr = "Test Description FR",
                    CategoryID = Guid.NewGuid(),
                    Variants = new List<ItemVariantDto>(),
                    ItemAttributes = new List<ItemAttributeDto>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                }
            };

            var result = Result.Success<IEnumerable<GetItemResponse>>(items);
            _mockItemService.Setup(x => x.GetAllItemsFromSellerAsync(sellerId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.GetSellerItems(sellerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetSellerItems_ReturnOk_WhenSellerHasNoItems()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var items = new List<GetItemResponse>();

            var result = Result.Success<IEnumerable<GetItemResponse>>(items);
            _mockItemService.Setup(x => x.GetAllItemsFromSellerAsync(sellerId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.GetSellerItems(sellerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetSellerItems_ReturnInternalServerError_WhenServiceFails()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var result = Result.Failure<IEnumerable<GetItemResponse>>("An error occurred", StatusCodes.Status500InternalServerError);
            _mockItemService.Setup(x => x.GetAllItemsFromSellerAsync(sellerId))
                           .ReturnsAsync(result);

            // Act
            var response = await _controller.GetSellerItems(sellerId);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, errorResult.StatusCode);
        }
    }
}