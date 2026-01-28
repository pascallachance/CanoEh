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
    public class CategoryNodeControllerShould
    {
        private readonly Mock<ICategoryNodeService> _mockCategoryNodeService;
        private readonly CategoryNodeController _controller;

        public CategoryNodeControllerShould()
        {
            _mockCategoryNodeService = new Mock<ICategoryNodeService>();
            _controller = new CategoryNodeController(_mockCategoryNodeService.Object);
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
        public async Task CreateCategoryNode_ReturnOk_WhenDepartementNodeCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Electronics Department",
                Name_fr = "Département Électronique",
                NodeType = "Departement",
                ParentId = null,
                IsActive = true,
                SortOrder = 1
            };

            var response = new CreateCategoryNodeResponse
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
            _mockCategoryNodeService.Setup(x => x.CreateCategoryNodeAsync(It.IsAny<CreateCategoryNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateCategoryNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateCategoryNode_ReturnOk_WhenNavigationNodeCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var parentId = Guid.NewGuid();
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Home Audio",
                Name_fr = "Audio Maison",
                NodeType = "Navigation",
                ParentId = parentId,
                IsActive = true,
                SortOrder = 1
            };

            var response = new CreateCategoryNodeResponse
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
            _mockCategoryNodeService.Setup(x => x.CreateCategoryNodeAsync(It.IsAny<CreateCategoryNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateCategoryNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateCategoryNode_ReturnOk_WhenCategoryNodeCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var parentId = Guid.NewGuid();
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = parentId,
                IsActive = true,
                SortOrder = 1
            };

            var response = new CreateCategoryNodeResponse
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
            _mockCategoryNodeService.Setup(x => x.CreateCategoryNodeAsync(It.IsAny<CreateCategoryNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateCategoryNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task CreateCategoryNode_ReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            SetupAdminUser();
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "",
                Name_fr = "",
                NodeType = "InvalidType",
                ParentId = null,
                IsActive = true
            };

            var result = Result.Failure<CreateCategoryNodeResponse>("Validation failed.", StatusCodes.Status400BadRequest);
            _mockCategoryNodeService.Setup(x => x.CreateCategoryNodeAsync(It.IsAny<CreateCategoryNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.CreateCategoryNode(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreateCategoryNode_ReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            SetupAdminUser();
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Test Node",
                Name_fr = "Noeud Test",
                NodeType = "Departement",
                ParentId = null,
                IsActive = true
            };

            _mockCategoryNodeService.Setup(x => x.CreateCategoryNodeAsync(It.IsAny<CreateCategoryNodeRequest>()))
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var actionResult = await _controller.CreateCategoryNode(request);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
        }

        [Fact]
        public async Task GetAllCategoryNodes_ReturnOk_WhenNodesExist()
        {
            // Arrange
            var nodes = new List<GetCategoryNodeResponse>
            {
                new GetCategoryNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
                    NodeType = "Departement",
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetCategoryNodeResponse>()
                },
                new GetCategoryNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Clothing",
                    Name_fr = "Vêtements",
                    NodeType = "Departement",
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 2,
                    Children = new List<GetCategoryNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryNodeResponse>>(nodes);
            _mockCategoryNodeService.Setup(x => x.GetAllCategoryNodesAsync())
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetAllCategoryNodes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetCategoryNodeById_ReturnOk_WhenNodeExists()
        {
            // Arrange
            var nodeId = Guid.NewGuid();
            var node = new GetCategoryNodeResponse
            {
                Id = nodeId,
                Name_en = "Electronics",
                Name_fr = "Électronique",
                NodeType = "Departement",
                ParentId = null,
                IsActive = true,
                SortOrder = 1,
                Children = new List<GetCategoryNodeResponse>()
            };

            var result = Result.Success(node);
            _mockCategoryNodeService.Setup(x => x.GetCategoryNodeByIdAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetCategoryNodeById(nodeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetCategoryNodeById_ReturnNotFound_WhenNodeDoesNotExist()
        {
            // Arrange
            var nodeId = Guid.NewGuid();
            var result = Result.Failure<GetCategoryNodeResponse>("Node not found.", StatusCodes.Status404NotFound);
            _mockCategoryNodeService.Setup(x => x.GetCategoryNodeByIdAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetCategoryNodeById(nodeId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetRootNodes_ReturnOk_WhenRootNodesExist()
        {
            // Arrange
            var rootNodes = new List<GetCategoryNodeResponse>
            {
                new GetCategoryNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
                    NodeType = "Departement",
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetCategoryNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryNodeResponse>>(rootNodes);
            _mockCategoryNodeService.Setup(x => x.GetRootNodesAsync())
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
            var children = new List<GetCategoryNodeResponse>
            {
                new GetCategoryNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Home Audio",
                    Name_fr = "Audio Maison",
                    NodeType = "Navigation",
                    ParentId = parentId,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetCategoryNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryNodeResponse>>(children);
            _mockCategoryNodeService.Setup(x => x.GetChildrenAsync(parentId))
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
            var nodes = new List<GetCategoryNodeResponse>
            {
                new GetCategoryNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Electronics",
                    Name_fr = "Électronique",
                    NodeType = nodeType,
                    ParentId = null,
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetCategoryNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryNodeResponse>>(nodes);
            _mockCategoryNodeService.Setup(x => x.GetNodesByTypeAsync(nodeType))
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
            var categoryNodes = new List<GetCategoryNodeResponse>
            {
                new GetCategoryNodeResponse
                {
                    Id = Guid.NewGuid(),
                    Name_en = "Speakers",
                    Name_fr = "Haut-parleurs",
                    NodeType = "Category",
                    ParentId = Guid.NewGuid(),
                    IsActive = true,
                    SortOrder = 1,
                    Children = new List<GetCategoryNodeResponse>()
                }
            };

            var result = Result.Success<IEnumerable<GetCategoryNodeResponse>>(categoryNodes);
            _mockCategoryNodeService.Setup(x => x.GetCategoryNodesAsync())
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.GetCategoryNodes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCategoryNode_ReturnOk_WhenNodeUpdatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var request = new UpdateCategoryNodeRequest
            {
                Id = nodeId,
                Name_en = "Updated Electronics",
                Name_fr = "Électronique Mise à Jour",
                ParentId = null,
                IsActive = true,
                SortOrder = 1
            };

            var response = new UpdateCategoryNodeResponse
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
            _mockCategoryNodeService.Setup(x => x.UpdateCategoryNodeAsync(It.IsAny<UpdateCategoryNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateCategoryNode(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateCategoryNode_ReturnNotFound_WhenNodeDoesNotExist()
        {
            // Arrange
            SetupAdminUser();
            var request = new UpdateCategoryNodeRequest
            {
                Id = Guid.NewGuid(),
                Name_en = "Updated Node",
                Name_fr = "Noeud Mis à Jour",
                ParentId = null,
                IsActive = true,
                SortOrder = 1
            };

            var result = Result.Failure<UpdateCategoryNodeResponse>("Node not found.", StatusCodes.Status404NotFound);
            _mockCategoryNodeService.Setup(x => x.UpdateCategoryNodeAsync(It.IsAny<UpdateCategoryNodeRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.UpdateCategoryNode(request);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategoryNode_ReturnOk_WhenNodeDeletedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var response = new DeleteCategoryNodeResponse
            {
                Id = nodeId,
                Success = true,
                Message = "Category node deleted successfully."
            };

            var result = Result.Success(response);
            _mockCategoryNodeService.Setup(x => x.DeleteCategoryNodeAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.DeleteCategoryNode(nodeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategoryNode_ReturnNotFound_WhenNodeDoesNotExist()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var result = Result.Failure<DeleteCategoryNodeResponse>("Node not found.", StatusCodes.Status404NotFound);
            _mockCategoryNodeService.Setup(x => x.DeleteCategoryNodeAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.DeleteCategoryNode(nodeId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteCategoryNode_ReturnBadRequest_WhenNodeHasChildren()
        {
            // Arrange
            SetupAdminUser();
            var nodeId = Guid.NewGuid();
            var result = Result.Failure<DeleteCategoryNodeResponse>(
                "Cannot delete node that has children.", 
                StatusCodes.Status400BadRequest);
            _mockCategoryNodeService.Setup(x => x.DeleteCategoryNodeAsync(nodeId))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.DeleteCategoryNode(nodeId);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task BulkCreateCategoryNodes_ReturnOk_WhenStructureCreatedSuccessfully()
        {
            // Arrange
            SetupAdminUser();
            var request = new BulkCreateCategoryNodesRequest
            {
                Departements = new List<DepartementNodeDto>
                {
                    new DepartementNodeDto
                    {
                        Name_en = "Electronics",
                        Name_fr = "Électronique",
                        IsActive = true,
                        SortOrder = 1,
                        NavigationNodes = new List<NavigationNodeDto>
                        {
                            new NavigationNodeDto
                            {
                                Name_en = "Computers",
                                Name_fr = "Ordinateurs",
                                IsActive = true,
                                SortOrder = 1,
                                CategoryNodes = new List<CategoryNodeDto>
                                {
                                    new CategoryNodeDto
                                    {
                                        Name_en = "Laptops",
                                        Name_fr = "Ordinateurs portables",
                                        IsActive = true,
                                        SortOrder = 1
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var response = new BulkCreateCategoryNodesResponse
            {
                Departements = new List<DepartementNodeResponseDto>
                {
                    new DepartementNodeResponseDto
                    {
                        Id = Guid.NewGuid(),
                        Name_en = "Electronics",
                        Name_fr = "Électronique",
                        NodeType = "Departement",
                        IsActive = true,
                        SortOrder = 1,
                        NavigationNodes = new List<NavigationNodeResponseDto>
                        {
                            new NavigationNodeResponseDto
                            {
                                Id = Guid.NewGuid(),
                                Name_en = "Computers",
                                Name_fr = "Ordinateurs",
                                NodeType = "Navigation",
                                IsActive = true,
                                SortOrder = 1,
                                CategoryNodes = new List<CategoryNodeResponseDto>
                                {
                                    new CategoryNodeResponseDto
                                    {
                                        Id = Guid.NewGuid(),
                                        Name_en = "Laptops",
                                        Name_fr = "Ordinateurs portables",
                                        NodeType = "Category",
                                        IsActive = true,
                                        SortOrder = 1
                                    }
                                }
                            }
                        }
                    }
                },
                TotalNodesCreated = 3
            };

            var result = Result.Success(response);
            _mockCategoryNodeService.Setup(x => x.BulkCreateCategoryNodesAsync(It.IsAny<BulkCreateCategoryNodesRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.BulkCreateCategoryNodes(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var resultValue = Assert.IsType<Result<BulkCreateCategoryNodesResponse>>(okResult.Value);
            Assert.True(resultValue.IsSuccess);
            Assert.Equal(3, resultValue.Value?.TotalNodesCreated);
        }

        [Fact]
        public async Task BulkCreateCategoryNodes_ReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            SetupAdminUser();
            var request = new BulkCreateCategoryNodesRequest
            {
                Departements = new List<DepartementNodeDto>()
            };

            var result = Result.Failure<BulkCreateCategoryNodesResponse>(
                "At least one Departement node is required.", 
                StatusCodes.Status400BadRequest);
            _mockCategoryNodeService.Setup(x => x.BulkCreateCategoryNodesAsync(It.IsAny<BulkCreateCategoryNodesRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.BulkCreateCategoryNodes(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task BulkCreateCategoryNodes_ReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            SetupAdminUser();
            var request = new BulkCreateCategoryNodesRequest
            {
                Departements = new List<DepartementNodeDto>
                {
                    new DepartementNodeDto
                    {
                        Name_en = "Electronics",
                        Name_fr = "Électronique"
                    }
                }
            };

            _mockCategoryNodeService.Setup(x => x.BulkCreateCategoryNodesAsync(It.IsAny<BulkCreateCategoryNodesRequest>()))
                               .ThrowsAsync(new Exception("Database error"));

            // Act
            var actionResult = await _controller.BulkCreateCategoryNodes(request);

            // Assert
            var serverErrorResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status500InternalServerError, serverErrorResult.StatusCode);
        }

        [Fact]
        public async Task BulkCreateCategoryNodes_ReturnOk_WhenMultipleDepartmentsCreated()
        {
            // Arrange
            SetupAdminUser();
            var request = new BulkCreateCategoryNodesRequest
            {
                Departements = new List<DepartementNodeDto>
                {
                    new DepartementNodeDto
                    {
                        Name_en = "Electronics",
                        Name_fr = "Électronique",
                        IsActive = true,
                        SortOrder = 1
                    },
                    new DepartementNodeDto
                    {
                        Name_en = "Clothing",
                        Name_fr = "Vêtements",
                        IsActive = true,
                        SortOrder = 2
                    }
                }
            };

            var response = new BulkCreateCategoryNodesResponse
            {
                Departements = new List<DepartementNodeResponseDto>
                {
                    new DepartementNodeResponseDto
                    {
                        Id = Guid.NewGuid(),
                        Name_en = "Electronics",
                        Name_fr = "Électronique",
                        NodeType = "Departement",
                        IsActive = true,
                        SortOrder = 1
                    },
                    new DepartementNodeResponseDto
                    {
                        Id = Guid.NewGuid(),
                        Name_en = "Clothing",
                        Name_fr = "Vêtements",
                        NodeType = "Departement",
                        IsActive = true,
                        SortOrder = 2
                    }
                },
                TotalNodesCreated = 2
            };

            var result = Result.Success(response);
            _mockCategoryNodeService.Setup(x => x.BulkCreateCategoryNodesAsync(It.IsAny<BulkCreateCategoryNodesRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.BulkCreateCategoryNodes(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            var resultValue = Assert.IsType<Result<BulkCreateCategoryNodesResponse>>(okResult.Value);
            Assert.True(resultValue.IsSuccess);
            Assert.Equal(2, resultValue.Value?.Departements.Count);
            Assert.Equal(2, resultValue.Value?.TotalNodesCreated);
        }

        [Fact]
        public async Task BulkCreateCategoryNodes_ReturnBadRequest_WhenAttributeTypeTooLong()
        {
            // Arrange
            SetupAdminUser();
            var longAttributeType = new string('X', 51); // Max is 50 characters
            var request = new BulkCreateCategoryNodesRequest
            {
                Departements = new List<DepartementNodeDto>
                {
                    new DepartementNodeDto
                    {
                        Name_en = "Electronics",
                        Name_fr = "Électronique",
                        NavigationNodes = new List<NavigationNodeDto>
                        {
                            new NavigationNodeDto
                            {
                                Name_en = "Computers",
                                Name_fr = "Ordinateurs",
                                CategoryNodes = new List<CategoryNodeDto>
                                {
                                    new CategoryNodeDto
                                    {
                                        Name_en = "Laptops",
                                        Name_fr = "Ordinateurs portables",
                                        CategoryMandatoryAttributes = new List<CreateCategoryMandatoryAttributeDto>
                                        {
                                            new CreateCategoryMandatoryAttributeDto
                                            {
                                                Name_en = "Brand",
                                                Name_fr = "Marque",
                                                AttributeType = longAttributeType
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var result = Result.Failure<BulkCreateCategoryNodesResponse>(
                "CategoryMandatoryAttribute AttributeType cannot exceed 50 characters.",
                StatusCodes.Status400BadRequest);
            _mockCategoryNodeService.Setup(x => x.BulkCreateCategoryNodesAsync(It.IsAny<BulkCreateCategoryNodesRequest>()))
                               .ReturnsAsync(result);

            // Act
            var actionResult = await _controller.BulkCreateCategoryNodes(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }
    }
}
