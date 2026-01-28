using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class CategoryNodeService(ICategoryNodeRepository categoryNodeRepository) : ICategoryNodeService
    {
        private readonly ICategoryNodeRepository _categoryNodeRepository = categoryNodeRepository;

        public async Task<Result<CreateCategoryNodeResponse>> CreateCategoryNodeAsync(CreateCategoryNodeRequest request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateCategoryNodeResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                // Validate parent exists if provided
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _categoryNodeRepository.ExistsAsync(request.ParentId.Value);
                    if (!parentExists)
                    {
                        return Result.Failure<CreateCategoryNodeResponse>(
                            "Parent node does not exist.", 
                            StatusCodes.Status400BadRequest);
                    }
                }

                BaseNode node = request.NodeType switch
                {
                    var type when type == BaseNode.NodeTypeDepartement => new DepartementNode(),
                    var type when type == BaseNode.NodeTypeNavigation => new NavigationNode(),
                    var type when type == BaseNode.NodeTypeCategory => new CategoryNode(),
                    _ => throw new ArgumentException("Invalid NodeType")
                };

                node.Id = Guid.NewGuid();
                node.Name_en = request.Name_en;
                node.Name_fr = request.Name_fr;
                node.ParentId = request.ParentId;
                node.IsActive = request.IsActive;
                node.SortOrder = request.SortOrder;
                node.CreatedAt = DateTime.UtcNow;

                // Prepare CategoryMandatoryAttributes if this is a Category node and attributes are provided
                var attributesToCreate = new List<CategoryMandatoryAttribute>();
                if (request.NodeType == BaseNode.NodeTypeCategory && 
                    request.CategoryMandatoryAttributes != null && 
                    request.CategoryMandatoryAttributes.Any())
                {
                    foreach (var attrDto in request.CategoryMandatoryAttributes)
                    {
                        attributesToCreate.Add(new CategoryMandatoryAttribute
                        {
                            Id = Guid.NewGuid(),
                            CategoryNodeId = node.Id,
                            Name_en = attrDto.Name_en,
                            Name_fr = attrDto.Name_fr,
                            AttributeType = attrDto.AttributeType,
                            SortOrder = attrDto.SortOrder
                        });
                    }
                }

                // Create node and attributes in a single transaction
                BaseNode createdNode;
                IEnumerable<CategoryMandatoryAttribute> createdAttributes;

                if (attributesToCreate.Any())
                {
                    (createdNode, createdAttributes) = await _categoryNodeRepository.AddNodeWithAttributesAsync(node, attributesToCreate);
                }
                else
                {
                    createdNode = await _categoryNodeRepository.AddAsync(node);
                    createdAttributes = new List<CategoryMandatoryAttribute>();
                }

                var response = new CreateCategoryNodeResponse
                {
                    Id = createdNode.Id,
                    Name_en = createdNode.Name_en,
                    Name_fr = createdNode.Name_fr,
                    NodeType = createdNode.NodeType,
                    ParentId = createdNode.ParentId,
                    IsActive = createdNode.IsActive,
                    SortOrder = createdNode.SortOrder,
                    CategoryMandatoryAttributes = createdAttributes.Select(attr => new CategoryMandatoryAttributeResponseDto
                    {
                        Id = attr.Id,
                        Name_en = attr.Name_en,
                        Name_fr = attr.Name_fr,
                        AttributeType = attr.AttributeType,
                        SortOrder = attr.SortOrder
                    }).ToList()
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateCategoryNodeResponse>(
                    $"An error occurred while creating the category node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<BulkCreateStructureResponse>> CreateStructureAsync(BulkCreateStructureRequest request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<BulkCreateStructureResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                var nodesWithAttributes = new List<(BaseNode node, IEnumerable<CategoryMandatoryAttribute> attributes)>();
                var responseDepartements = new List<DepartementNodeResponseDto>();
                var totalNodesCreated = 0;

                // Process each departement
                foreach (var deptDto in request.Departements)
                {
                    var deptResponse = ProcessDepartementNode(deptDto, nodesWithAttributes, ref totalNodesCreated);
                    responseDepartements.Add(deptResponse);
                }

                // Create all nodes in a single transaction
                await _categoryNodeRepository.AddMultipleNodesWithAttributesAsync(nodesWithAttributes);

                var response = new BulkCreateStructureResponse
                {
                    Departements = responseDepartements,
                    TotalNodesCreated = totalNodesCreated
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<BulkCreateStructureResponse>(
                    $"An error occurred while creating the structure: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        private DepartementNodeResponseDto ProcessDepartementNode(
            DepartementNodeDto deptDto,
            List<(BaseNode node, IEnumerable<CategoryMandatoryAttribute> attributes)> nodesWithAttributes,
            ref int totalNodesCreated)
        {
            var dept = new DepartementNode
            {
                Id = Guid.NewGuid(),
                Name_en = deptDto.Name_en,
                Name_fr = deptDto.Name_fr,
                ParentId = null, // Departement nodes never have a parent
                IsActive = deptDto.IsActive,
                SortOrder = deptDto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };

            nodesWithAttributes.Add((dept, new List<CategoryMandatoryAttribute>()));
            totalNodesCreated++;

            var response = new DepartementNodeResponseDto
            {
                Id = dept.Id,
                Name_en = dept.Name_en,
                Name_fr = dept.Name_fr,
                NodeType = dept.NodeType,
                IsActive = dept.IsActive,
                SortOrder = dept.SortOrder,
                NavigationNodes = new List<NavigationNodeResponseDto>(),
                CategoryNodes = new List<CategoryNodeResponseDto>()
            };

            // Process navigation nodes
            if (deptDto.NavigationNodes != null)
            {
                foreach (var navDto in deptDto.NavigationNodes)
                {
                    var navResponse = ProcessNavigationNode(navDto, dept.Id, nodesWithAttributes, ref totalNodesCreated);
                    response.NavigationNodes.Add(navResponse);
                }
            }

            // Process category nodes
            if (deptDto.CategoryNodes != null)
            {
                foreach (var catDto in deptDto.CategoryNodes)
                {
                    var catResponse = ProcessCategoryNode(catDto, dept.Id, nodesWithAttributes, ref totalNodesCreated);
                    response.CategoryNodes.Add(catResponse);
                }
            }

            return response;
        }

        private NavigationNodeResponseDto ProcessNavigationNode(
            NavigationNodeDto navDto,
            Guid parentId,
            List<(BaseNode node, IEnumerable<CategoryMandatoryAttribute> attributes)> nodesWithAttributes,
            ref int totalNodesCreated)
        {
            var nav = new NavigationNode
            {
                Id = Guid.NewGuid(),
                Name_en = navDto.Name_en,
                Name_fr = navDto.Name_fr,
                ParentId = parentId,
                IsActive = navDto.IsActive,
                SortOrder = navDto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };

            nodesWithAttributes.Add((nav, new List<CategoryMandatoryAttribute>()));
            totalNodesCreated++;

            var response = new NavigationNodeResponseDto
            {
                Id = nav.Id,
                Name_en = nav.Name_en,
                Name_fr = nav.Name_fr,
                NodeType = nav.NodeType,
                IsActive = nav.IsActive,
                SortOrder = nav.SortOrder,
                NavigationNodes = new List<NavigationNodeResponseDto>(),
                CategoryNodes = new List<CategoryNodeResponseDto>()
            };

            // Process child navigation nodes (recursive)
            if (navDto.NavigationNodes != null)
            {
                foreach (var childNavDto in navDto.NavigationNodes)
                {
                    var childNavResponse = ProcessNavigationNode(childNavDto, nav.Id, nodesWithAttributes, ref totalNodesCreated);
                    response.NavigationNodes.Add(childNavResponse);
                }
            }

            // Process category nodes
            if (navDto.CategoryNodes != null)
            {
                foreach (var catDto in navDto.CategoryNodes)
                {
                    var catResponse = ProcessCategoryNode(catDto, nav.Id, nodesWithAttributes, ref totalNodesCreated);
                    response.CategoryNodes.Add(catResponse);
                }
            }

            return response;
        }

        private CategoryNodeResponseDto ProcessCategoryNode(
            CategoryNodeDto catDto,
            Guid parentId,
            List<(BaseNode node, IEnumerable<CategoryMandatoryAttribute> attributes)> nodesWithAttributes,
            ref int totalNodesCreated)
        {
            var cat = new CategoryNode
            {
                Id = Guid.NewGuid(),
                Name_en = catDto.Name_en,
                Name_fr = catDto.Name_fr,
                ParentId = parentId,
                IsActive = catDto.IsActive,
                SortOrder = catDto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };

            var attributes = new List<CategoryMandatoryAttribute>();
            if (catDto.CategoryMandatoryAttributes != null && catDto.CategoryMandatoryAttributes.Any())
            {
                foreach (var attrDto in catDto.CategoryMandatoryAttributes)
                {
                    attributes.Add(new CategoryMandatoryAttribute
                    {
                        Id = Guid.NewGuid(),
                        CategoryNodeId = cat.Id,
                        Name_en = attrDto.Name_en,
                        Name_fr = attrDto.Name_fr,
                        AttributeType = attrDto.AttributeType,
                        SortOrder = attrDto.SortOrder
                    });
                }
            }

            nodesWithAttributes.Add((cat, attributes));
            totalNodesCreated++;

            var response = new CategoryNodeResponseDto
            {
                Id = cat.Id,
                Name_en = cat.Name_en,
                Name_fr = cat.Name_fr,
                NodeType = cat.NodeType,
                IsActive = cat.IsActive,
                SortOrder = cat.SortOrder,
                CategoryMandatoryAttributes = attributes.Select(attr => new CategoryMandatoryAttributeResponseDto
                {
                    Id = attr.Id,
                    Name_en = attr.Name_en,
                    Name_fr = attr.Name_fr,
                    AttributeType = attr.AttributeType,
                    SortOrder = attr.SortOrder
                }).ToList()
            };

            return response;
        }

        public async Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetAllCategoryNodesAsync()
        {
            try
            {
                var nodes = await _categoryNodeRepository.GetAllAsync();
                var response = nodes.Select(MapToGetCategoryNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                    $"An error occurred while retrieving category nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetCategoryNodeResponse>> GetCategoryNodeByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<GetCategoryNodeResponse>(
                        "Category node ID cannot be empty.",
                        StatusCodes.Status400BadRequest);
                }

                var node = await _categoryNodeRepository.GetNodeByIdAsync(id);
                if (node == null)
                {
                    return Result.Failure<GetCategoryNodeResponse>(
                        "Category node not found.",
                        StatusCodes.Status404NotFound);
                }

                // Get children
                var children = await _categoryNodeRepository.GetChildrenAsync(id);

                var response = MapToGetCategoryNodeResponse(node, children);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<GetCategoryNodeResponse>(
                    $"An error occurred while retrieving the category node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetRootNodesAsync()
        {
            try
            {
                var nodes = await _categoryNodeRepository.GetRootNodesAsync();
                var response = nodes.Select(MapToGetCategoryNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                    $"An error occurred while retrieving root nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetChildrenAsync(Guid parentId)
        {
            try
            {
                if (parentId == Guid.Empty)
                {
                    return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                        "Parent ID cannot be empty.",
                        StatusCodes.Status400BadRequest);
                }

                var parentExists = await _categoryNodeRepository.ExistsAsync(parentId);
                if (!parentExists)
                {
                    return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                        "Parent node does not exist.",
                        StatusCodes.Status404NotFound);
                }

                var nodes = await _categoryNodeRepository.GetChildrenAsync(parentId);
                var response = nodes.Select(MapToGetCategoryNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                    $"An error occurred while retrieving child nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetNodesByTypeAsync(string nodeType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nodeType))
                {
                    return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                        "NodeType is required.",
                        StatusCodes.Status400BadRequest);
                }

                if (nodeType != BaseNode.NodeTypeDepartement && nodeType != BaseNode.NodeTypeNavigation && nodeType != BaseNode.NodeTypeCategory)
                {
                    return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                        $"NodeType must be '{BaseNode.NodeTypeDepartement}', '{BaseNode.NodeTypeNavigation}', or '{BaseNode.NodeTypeCategory}'.",
                        StatusCodes.Status400BadRequest);
                }

                var nodes = await _categoryNodeRepository.GetNodesByTypeAsync(nodeType);
                var response = nodes.Select(MapToGetCategoryNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                    $"An error occurred while retrieving nodes by type: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetCategoryNodesAsync()
        {
            try
            {
                var nodes = await _categoryNodeRepository.GetCategoryNodesAsync();
                var response = nodes.Select(MapToGetCategoryNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryNodeResponse>>(
                    $"An error occurred while retrieving category nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateCategoryNodeResponse>> UpdateCategoryNodeAsync(UpdateCategoryNodeRequest request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdateCategoryNodeResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                var existingNode = await _categoryNodeRepository.GetNodeByIdAsync(request.Id);
                if (existingNode == null)
                {
                    return Result.Failure<UpdateCategoryNodeResponse>(
                        "Category node not found.",
                        StatusCodes.Status404NotFound);
                }

                // Validate parent exists if provided
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _categoryNodeRepository.ExistsAsync(request.ParentId.Value);
                    if (!parentExists)
                    {
                        return Result.Failure<UpdateCategoryNodeResponse>(
                            "Parent node does not exist.",
                            StatusCodes.Status400BadRequest);
                    }
                }

                existingNode.Name_en = request.Name_en;
                existingNode.Name_fr = request.Name_fr;
                existingNode.ParentId = request.ParentId;
                existingNode.IsActive = request.IsActive;
                existingNode.SortOrder = request.SortOrder;
                existingNode.UpdatedAt = DateTime.UtcNow;

                var updatedNode = await _categoryNodeRepository.UpdateAsync(existingNode);

                var response = new UpdateCategoryNodeResponse
                {
                    Id = updatedNode.Id,
                    Name_en = updatedNode.Name_en,
                    Name_fr = updatedNode.Name_fr,
                    NodeType = updatedNode.NodeType,
                    ParentId = updatedNode.ParentId,
                    IsActive = updatedNode.IsActive,
                    SortOrder = updatedNode.SortOrder
                };

                return Result.Success(response);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<UpdateCategoryNodeResponse>(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return Result.Failure<UpdateCategoryNodeResponse>(
                    $"An error occurred while updating the category node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteCategoryNodeResponse>> DeleteCategoryNodeAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<DeleteCategoryNodeResponse>(
                        "Category node ID cannot be empty.",
                        StatusCodes.Status400BadRequest);
                }

                var node = await _categoryNodeRepository.GetNodeByIdAsync(id);
                if (node == null)
                {
                    return Result.Failure<DeleteCategoryNodeResponse>(
                        "Category node not found.",
                        StatusCodes.Status404NotFound);
                }

                await _categoryNodeRepository.DeleteAsync(node);

                var response = new DeleteCategoryNodeResponse
                {
                    Id = id,
                    Success = true,
                    Message = "Category node deleted successfully."
                };

                return Result.Success(response);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<DeleteCategoryNodeResponse>(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return Result.Failure<DeleteCategoryNodeResponse>(
                    $"An error occurred while deleting the category node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static GetCategoryNodeResponse MapToGetCategoryNodeResponse(BaseNode node)
        {
            return new GetCategoryNodeResponse
            {
                Id = node.Id,
                Name_en = node.Name_en,
                Name_fr = node.Name_fr,
                NodeType = node.NodeType,
                ParentId = node.ParentId,
                IsActive = node.IsActive,
                SortOrder = node.SortOrder,
                Children = new List<GetCategoryNodeResponse>()
            };
        }

        private static GetCategoryNodeResponse MapToGetCategoryNodeResponse(BaseNode node, IEnumerable<BaseNode> children)
        {
            return new GetCategoryNodeResponse
            {
                Id = node.Id,
                Name_en = node.Name_en,
                Name_fr = node.Name_fr,
                NodeType = node.NodeType,
                ParentId = node.ParentId,
                IsActive = node.IsActive,
                SortOrder = node.SortOrder,
                Children = children.Select(MapToGetCategoryNodeResponse).ToList()
            };
        }
    }
}
