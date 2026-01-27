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
    public class ProductNodeControllerShould
    {
        private readonly Mock<IProductNodeService> _mockProductNodeService;
        private readonly ProductNodeController _controller;

        public ProductNodeControllerShould()
        {
            _mockProductNodeService = new Mock<IProductNodeService>();
            _controller = new ProductNodeController(_mockProductNodeService.Object);
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
        public async Task CreateProductNode_ReturnOk_WhenDepartementNodeCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var request = new CreateProductNodeRequest
            {
                Name_en = "Electronics Department",
                Name_fr = "Département Électronique",
                NodeType = "Departement",
                ParentId = null,
                IsActive = true,
                SortOrder = 1
            };

            var response = new CreateProductNodeResponse
            {
                Id = Guid.NewGuid(),
                Name_en = request.Name_en,
                Name_fr = request.Name_fr,
                NodeType = request.NodeType,
                ParentId = request.ParentId,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder
            };

            var result = Result.Success(response);
            _mockProductNodeService.Setup(x => x.CreateProductNodeAsync(It.IsAny<CreateProductNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateProductNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateProductNode_ReturnOk_WhenNavigationNodeCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var parentId = Guid.NewGuid();
            var request = new CreateProductNodeRequest
            {
                Name_en = "Home Audio",
                Name_fr = "Audio Maison",
                NodeType = "Navigation",
                ParentId = parentId,
                IsActive = true,
                SortOrder = 1
            };

            var response = new CreateProductNodeResponse
            {
                Id = Guid.NewGuid(),
                Name_en = request.Name_en,
                Name_fr = request.Name_fr,
                NodeType = request.NodeType,
                ParentId = request.ParentId,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder
            };

            var result = Result.Success(response);
            _mockProductNodeService.Setup(x => x.CreateProductNodeAsync(It.IsAny<CreateProductNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateProductNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateProductNode_ReturnOk_WhenCategoryNodeCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var parentId = Guid.NewGuid();
            var request = new CreateProductNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = parentId,
                IsActive = true,
                SortOrder = 1
            };

            var response = new CreateProductNodeResponse
            {
                Id = Guid.NewGuid(),
                Name_en = request.Name_en,
                Name_fr = request.Name_fr,
                NodeType = request.NodeType,
                ParentId = request.ParentId,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder
            };

            var result = Result.Success(response);
            _mockProductNodeService.Setup(x => x.CreateProductNodeAsync(It.IsAny<CreateProductNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateProductNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateProductNode_ReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            SetupAdminUser();
            var request = new CreateProductNodeRequest
            {
                Name_en = "",
                Name_fr = "",
                NodeType = "InvalidType",
                ParentId = null,
                IsActive = true
            };

            var result = Result.Failure<CreateProductNodeResponse>("Validation failed.", StatusCodes.Status400BadRequest);
            _mockProductNodeService.Setup(x => x.CreateProductNodeAsync(It.IsAny<CreateProductNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateProductNode(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateProductNode_ReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            SetupAdminUser();
            var request = new CreateProductNodeRequest
            {
                Name_en = "Test Node",
                Name_fr = "Noeud Test",
                NodeType = "Departement",
                ParentId = null,
                IsActive = true
            };

            _mockProductNodeService.Setup(x => x.CreateProductNodeAsync(It.IsAny<CreateProductNodeRequest>()))
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var actionResult = await _controller.CreateProductNode(request);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
        }

        [Fact]
        public async Task GetAllProductNodes_ReturnOk_WhenNodesExist()
        {
            // Arrange
            var nodes = new List<GetProductNodeResponse>
            {
                new GetProductNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
                    NodeType = "Departement",
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetProductNodeResponse>()
                },
                new GetProductNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Clothing",
                    Name_fr = "Vêtements",
                    NodeType = "Departement",
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 2,
                    Children = new List<GetProductNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetProductNodeResponse>>(nodes);
            _mockProductNodeService.Setup(x => x.GetAllProductNodesAsync())
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetAllProductNodes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetProductNodeById_ReturnOk_WhenNodeExists()
        {
            // Arrange
            var nodeId = Guid.NewGuid();
            var node = new GetProductNodeResponse
            {
                Id = nodeId,
                Name_en = "Electronics",
                Name_fr = "Électronique",
                NodeType = "Departement",
                ParentId = null,
                IsActive = true,
                SortOrder = 1,
                Children = new List<GetProductNodeResponse>()
            };

            var result = Result.Success(node);
            _mockProductNodeService.Setup(x => x.GetProductNodeByIdAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetProductNodeById(nodeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetProductNodeById_ReturnNotFound_WhenNodeDoesNotExist()
        {
            // Arrange
            var nodeId = Guid.NewGuid();
            var result = Result.Failure<GetProductNodeResponse>("Node not found.", StatusCodes.Status404NotFound);
            _mockProductNodeService.Setup(x => x.GetProductNodeByIdAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetProductNodeById(nodeId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetRootNodes_ReturnOk_WhenRootNodesExist()
        {
            // Arrange
            var rootNodes = new List<GetProductNodeResponse>
            {
                new GetProductNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
                    NodeType = "Departement",
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetProductNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetProductNodeResponse>>(rootNodes);
            _mockProductNodeService.Setup(x => x.GetRootNodesAsync())
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetRootNodes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetChildren_ReturnOk_WhenChildrenExist()
        {
            // Arrange
            var parentId = Guid.NewGuid();
            var children = new List<GetProductNodeResponse>
            {
                new GetProductNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Home Audio",
                    Name_fr = "Audio Maison",
                    NodeType = "Navigation",
                    ParentId = parentId,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetProductNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetProductNodeResponse>>(children);
            _mockProductNodeService.Setup(x => x.GetChildrenAsync(parentId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetChildren(parentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetNodesByType_ReturnOk_WhenNodesOfTypeExist()
        {
            // Arrange
            var nodeType = "Departement";
            var nodes = new List<GetProductNodeResponse>
            {
                new GetProductNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
                    NodeType = nodeType,
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetProductNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetProductNodeResponse>>(nodes);
            _mockProductNodeService.Setup(x => x.GetNodesByTypeAsync(nodeType))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetNodesByType(nodeType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetCategoryNodes_ReturnOk_WhenCategoryNodesExist()
        {
            // Arrange
            var categoryNodes = new List<GetProductNodeResponse>
            {
                new GetProductNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Speakers",
                    Name_fr = "Haut-parleurs",
                    NodeType = "Category",
                    ParentId = Guid.NewGuid(),
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetProductNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetProductNodeResponse>>(categoryNodes);
            _mockProductNodeService.Setup(x => x.GetCategoryNodesAsync())
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetCategoryNodes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProductNode_ReturnOk_WhenNodeUpdatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var request = new UpdateProductNodeRequest
            {
                Id = nodeId,
                Name_en = "Updated Electronics",
                Name_fr = "Électronique Mise à Jour",
                ParentId = null,
                IsActive = true,
                SortOrder = 1
            };

            var response = new UpdateProductNodeResponse
            {
                Id = nodeId,
                Name_en = request.Name_en,
                Name_fr = request.Name_fr,
                NodeType = "Departement",
                ParentId = request.ParentId,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder
            };

            var result = Result.Success(response);
            _mockProductNodeService.Setup(x => x.UpdateProductNodeAsync(It.IsAny<UpdateProductNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateProductNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateProductNode_ReturnNotFound_WhenNodeDoesNotExist()
        {
            // Arrange
            SetupAdminUser();
            var request = new UpdateProductNodeRequest
            {
                Id = Guid.NewGuid(),
                Name_en = "Updated Node",
                Name_fr = "Noeud Mis à Jour",
                ParentId = null,
                IsActive = true,
                SortOrder = 1
            };

            var result = Result.Failure<UpdateProductNodeResponse>("Node not found.", StatusCodes.Status404NotFound);
            _mockProductNodeService.Setup(x => x.UpdateProductNodeAsync(It.IsAny<UpdateProductNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateProductNode(request);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProductNode_ReturnOk_WhenNodeDeletedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var response = new DeleteProductNodeResponse
            {
                Id = nodeId,
                Success = true,
                Message = "Product node deleted successfully."
            };

            var result = Result.Success(response);
            _mockProductNodeService.Setup(x => x.DeleteProductNodeAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.DeleteProductNode(nodeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProductNode_ReturnNotFound_WhenNodeDoesNotExist()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var result = Result.Failure<DeleteProductNodeResponse>("Node not found.", StatusCodes.Status404NotFound);
            _mockProductNodeService.Setup(x => x.DeleteProductNodeAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.DeleteProductNode(nodeId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteProductNode_ReturnBadRequest_WhenNodeHasChildren()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var result = Result.Failure<DeleteProductNodeResponse>(
                "Cannot delete node that has children.", 
                StatusCodes.Status400BadRequest);
            _mockProductNodeService.Setup(x => x.DeleteProductNodeAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.DeleteProductNode(nodeId);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }
    }
}
