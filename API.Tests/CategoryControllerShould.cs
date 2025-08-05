using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests
{
    public class CategoryControllerShould
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoryController _controller;

        public CategoryControllerShould()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoryController(_mockCategoryService.Object);
        }

        [Fact]
        public async Task CreateCategory_ReturnOk_WhenCategoryCreatedSuccessfully()
        {
            // Arrange
            var createCategoryRequest = new CreateCategoryRequest
            {
                Name = "Test Category",
                ParentCategoryId = null
            };

            var createCategoryResponse = new CreateCategoryResponse
            {
                Id = Guid.NewGuid(),
                Name = createCategoryRequest.Name,
                ParentCategoryId = createCategoryRequest.ParentCategoryId
            };

            var result = Result.Success(createCategoryResponse);
            _mockCategoryService.Setup(x => x.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>()))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.CreateCategory(createCategoryRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_ReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var createCategoryRequest = new CreateCategoryRequest
            {
                Name = "", // Invalid Name
                ParentCategoryId = null
            };

            var result = Result.Failure<CreateCategoryResponse>("Name is required.", StatusCodes.Status400BadRequest);
            _mockCategoryService.Setup(x => x.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>()))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.CreateCategory(createCategoryRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_ReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var createCategoryRequest = new CreateCategoryRequest
            {
                Name = "Test Category",
                ParentCategoryId = null
            };

            _mockCategoryService.Setup(x => x.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>()))
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _controller.CreateCategory(createCategoryRequest);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
        }

        [Fact]
        public async Task GetAllCategories_ReturnOk_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<GetCategoryResponse>
            {
                new GetCategoryResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Electronics",
                    ParentCategoryId = null,
                    Subcategories = new List<GetCategoryResponse>()
                },
                new GetCategoryResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Clothing",
                    ParentCategoryId = null,
                    Subcategories = new List<GetCategoryResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryResponse>>(categories);
            _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.GetAllCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetAllCategories_ReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _controller.GetAllCategories();

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
        }

        [Fact]
        public async Task GetCategoryById_ReturnOk_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryResponse = new GetCategoryResponse
            {
                Id = categoryId,
                Name = "Test Category",
                ParentCategoryId = null,
                Subcategories = new List<GetCategoryResponse>()
            };

            var result = Result.Success(categoryResponse);
            _mockCategoryService.Setup(x => x.GetCategoryByIdAsync(categoryId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.GetCategoryById(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetCategoryById_ReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var result = Result.Failure<GetCategoryResponse>("Category not found.", StatusCodes.Status404NotFound);
            _mockCategoryService.Setup(x => x.GetCategoryByIdAsync(categoryId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.GetCategoryById(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetRootCategories_ReturnOk_WhenRootCategoriesExist()
        {
            // Arrange
            var rootCategories = new List<GetCategoryResponse>
            {
                new GetCategoryResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Electronics",
                    ParentCategoryId = null,
                    Subcategories = new List<GetCategoryResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryResponse>>(rootCategories);
            _mockCategoryService.Setup(x => x.GetRootCategoriesAsync())
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.GetRootCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetSubcategories_ReturnOk_WhenSubcategoriesExist()
        {
            // Arrange
            var parentCategoryId = Guid.NewGuid();
            var subcategories = new List<GetCategoryResponse>
            {
                new GetCategoryResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Smartphones",
                    ParentCategoryId = parentCategoryId,
                    Subcategories = new List<GetCategoryResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryResponse>>(subcategories);
            _mockCategoryService.Setup(x => x.GetSubcategoriesAsync(parentCategoryId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.GetSubcategories(parentCategoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetSubcategories_ReturnNotFound_WhenParentCategoryDoesNotExist()
        {
            // Arrange
            var parentCategoryId = Guid.NewGuid();
            var result = Result.Failure<IEnumerable<GetCategoryResponse>>("Parent category does not exist.", StatusCodes.Status404NotFound);
            _mockCategoryService.Setup(x => x.GetSubcategoriesAsync(parentCategoryId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.GetSubcategories(parentCategoryId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_ReturnOk_WhenCategoryUpdatedSuccessfully()
        {
            // Arrange
            var updateCategoryRequest = new UpdateCategoryRequest
            {
                Id = Guid.NewGuid(),
                Name = "Updated Category",
                ParentCategoryId = null
            };

            var updateCategoryResponse = new UpdateCategoryResponse
            {
                Id = updateCategoryRequest.Id,
                Name = updateCategoryRequest.Name,
                ParentCategoryId = updateCategoryRequest.ParentCategoryId
            };

            var result = Result.Success(updateCategoryResponse);
            _mockCategoryService.Setup(x => x.UpdateCategoryAsync(It.IsAny<UpdateCategoryRequest>()))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.UpdateCategory(updateCategoryRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_ReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var updateCategoryRequest = new UpdateCategoryRequest
            {
                Id = Guid.NewGuid(),
                Name = "Updated Category",
                ParentCategoryId = null
            };

            var result = Result.Failure<UpdateCategoryResponse>("Category not found.", StatusCodes.Status404NotFound);
            _mockCategoryService.Setup(x => x.UpdateCategoryAsync(It.IsAny<UpdateCategoryRequest>()))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.UpdateCategory(updateCategoryRequest);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCategory_ReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var updateCategoryRequest = new UpdateCategoryRequest
            {
                Id = Guid.Empty, // Invalid ID
                Name = "Updated Category",
                ParentCategoryId = null
            };

            var result = Result.Failure<UpdateCategoryResponse>("Id is required.", StatusCodes.Status400BadRequest);
            _mockCategoryService.Setup(x => x.UpdateCategoryAsync(It.IsAny<UpdateCategoryRequest>()))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.UpdateCategory(updateCategoryRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ReturnOk_WhenCategoryDeletedSuccessfully()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var deleteCategoryResponse = new DeleteCategoryResponse
            {
                Id = categoryId,
                Success = true,
                Message = "Category deleted successfully."
            };

            var result = Result.Success(deleteCategoryResponse);
            _mockCategoryService.Setup(x => x.DeleteCategoryAsync(categoryId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteCategory(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ReturnNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var result = Result.Failure<DeleteCategoryResponse>("Category not found.", StatusCodes.Status404NotFound);
            _mockCategoryService.Setup(x => x.DeleteCategoryAsync(categoryId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteCategory(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ReturnBadRequest_WhenCategoryHasSubcategoriesOrItems()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var result = Result.Failure<DeleteCategoryResponse>("Cannot delete category that has subcategories.", StatusCodes.Status400BadRequest);
            _mockCategoryService.Setup(x => x.DeleteCategoryAsync(categoryId))
                               .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteCategory(categoryId);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _mockCategoryService.Setup(x => x.DeleteCategoryAsync(categoryId))
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _controller.DeleteCategory(categoryId);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
        }
    }
}