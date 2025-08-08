using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

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

        private void SetupAdminUser()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "admin@example.com"),
                new(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private void SetupUnauthenticatedUser()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task CreateCategory_ReturnOk_WhenCategoryCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var createCategoryRequest = new CreateCategoryRequest
            {
                Name_en = "Test Category",
                Name_fr = "Catégorie de Test",
                ParentCategoryId = null
            };

            var createCategoryResponse = new CreateCategoryResponse
            {
                Id = Guid.NewGuid(),
                Name_en = createCategoryRequest.Name_en,
                Name_fr = createCategoryRequest.Name_fr,
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
            SetupAdminUser();
            var createCategoryRequest = new CreateCategoryRequest
            {
                Name_en = "", // Invalid Name
                Name_fr = "", // Invalid Name
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
            SetupAdminUser();
            var createCategoryRequest = new CreateCategoryRequest
            {
                Name_en = "Test Category",
                Name_fr = "Catégorie de Test",
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
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
                    ParentCategoryId = null,
                    Subcategories = new List<GetCategoryResponse>()
                },
                new GetCategoryResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Clothing",
                    Name_fr = "Vêtements",
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
                Name_en = "Test Category",
                Name_fr = "Catégorie de Test",
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
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
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
                    Name_en = "Smartphones",
                    Name_fr = "Téléphones intelligents",
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
            SetupAdminUser();
            var updateCategoryRequest = new UpdateCategoryRequest
            {
                Id = Guid.NewGuid(),
                Name_en = "Updated Category",
                Name_fr = "Catégorie Mise à Jour",
                ParentCategoryId = null
            };

            var updateCategoryResponse = new UpdateCategoryResponse
            {
                Id = updateCategoryRequest.Id,
                Name_en = updateCategoryRequest.Name_en,
                Name_fr = updateCategoryRequest.Name_fr,
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
            SetupAdminUser();
            var updateCategoryRequest = new UpdateCategoryRequest
            {
                Id = Guid.NewGuid(),
                Name_en = "Updated Category",
                Name_fr = "Catégorie Mise à Jour",
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
            SetupAdminUser();
            var updateCategoryRequest = new UpdateCategoryRequest
            {
                Id = Guid.Empty, // Invalid ID
                Name_en = "Updated Category",
                Name_fr = "Catégorie Mise à Jour",
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
            SetupAdminUser();
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
            SetupAdminUser();
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
            SetupAdminUser();
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
            SetupAdminUser();
            var categoryId = Guid.NewGuid();
            _mockCategoryService.Setup(x => x.DeleteCategoryAsync(categoryId))
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _controller.DeleteCategory(categoryId);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
        }

        [Fact]
        public void CategoryEntity_ShouldHave_CreatedAtAndUpdatedAtProperties()
        {
            // Arrange & Act
            var category = new Infrastructure.Data.Category
            {
                Id = Guid.NewGuid(),
                Name_en = "Test Category",
                Name_fr = "Catégorie Test",
                ParentCategoryId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            // Assert
            Assert.NotEqual(default(DateTime), category.CreatedAt);
            Assert.IsType<DateTime>(category.CreatedAt);
            Assert.Null(category.UpdatedAt);
            
            // Test with UpdatedAt set
            category.UpdatedAt = DateTime.UtcNow;
            Assert.NotNull(category.UpdatedAt);
            Assert.True(category.UpdatedAt.HasValue);
            Assert.IsType<DateTime>(category.UpdatedAt.Value);
        }
    }
}