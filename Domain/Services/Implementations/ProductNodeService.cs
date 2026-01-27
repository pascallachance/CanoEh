using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class ProductNodeService(IProductNodeRepository productNodeRepository) : IProductNodeService
    {
        private readonly IProductNodeRepository _productNodeRepository = productNodeRepository;

        public async Task<Result<CreateProductNodeResponse>> CreateProductNodeAsync(CreateProductNodeRequest request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateProductNodeResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                // Validate parent exists if provided
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _productNodeRepository.ExistsAsync(request.ParentId.Value);
                    if (!parentExists)
                    {
                        return Result.Failure<CreateProductNodeResponse>(
                            "Parent node does not exist.", 
                            StatusCodes.Status400BadRequest);
                    }
                }

                BaseNode node = request.NodeType switch
                {
                    "Departement" => new DepartementNode(),
                    "Navigation" => new NavigationNode(),
                    "Category" => new CategoryNode(),
                    _ => throw new ArgumentException("Invalid NodeType")
                };

                node.Id = Guid.NewGuid();
                node.Name_en = request.Name_en;
                node.Name_fr = request.Name_fr;
                node.ParentId = request.ParentId;
                node.IsActive = request.IsActive;
                node.SortOrder = request.SortOrder;
                node.CreatedAt = DateTime.UtcNow;

                var createdNode = await _productNodeRepository.AddAsync(node);

                var response = new CreateProductNodeResponse
                {
                    Id = createdNode.Id,
                    Name_en = createdNode.Name_en,
                    Name_fr = createdNode.Name_fr,
                    NodeType = createdNode.NodeType,
                    ParentId = createdNode.ParentId,
                    IsActive = createdNode.IsActive,
                    SortOrder = createdNode.SortOrder
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateProductNodeResponse>(
                    $"An error occurred while creating the product node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetProductNodeResponse>>> GetAllProductNodesAsync()
        {
            try
            {
                var nodes = await _productNodeRepository.GetAllAsync();
                var response = nodes.Select(MapToGetProductNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                    $"An error occurred while retrieving product nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetProductNodeResponse>> GetProductNodeByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<GetProductNodeResponse>(
                        "Product node ID is required.",
                        StatusCodes.Status400BadRequest);
                }

                var node = await _productNodeRepository.GetNodeByIdAsync(id);
                if (node == null)
                {
                    return Result.Failure<GetProductNodeResponse>(
                        "Product node not found.",
                        StatusCodes.Status404NotFound);
                }

                // Get children
                var children = await _productNodeRepository.GetChildrenAsync(id);

                var response = MapToGetProductNodeResponse(node, children);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<GetProductNodeResponse>(
                    $"An error occurred while retrieving the product node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetProductNodeResponse>>> GetRootNodesAsync()
        {
            try
            {
                var nodes = await _productNodeRepository.GetRootNodesAsync();
                var response = nodes.Select(MapToGetProductNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                    $"An error occurred while retrieving root nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetProductNodeResponse>>> GetChildrenAsync(Guid parentId)
        {
            try
            {
                if (parentId == Guid.Empty)
                {
                    return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                        "Parent ID is required.",
                        StatusCodes.Status400BadRequest);
                }

                var parentExists = await _productNodeRepository.ExistsAsync(parentId);
                if (!parentExists)
                {
                    return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                        "Parent node does not exist.",
                        StatusCodes.Status404NotFound);
                }

                var nodes = await _productNodeRepository.GetChildrenAsync(parentId);
                var response = nodes.Select(MapToGetProductNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                    $"An error occurred while retrieving child nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetProductNodeResponse>>> GetNodesByTypeAsync(string nodeType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nodeType))
                {
                    return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                        "NodeType is required.",
                        StatusCodes.Status400BadRequest);
                }

                if (nodeType != "Departement" && nodeType != "Navigation" && nodeType != "Category")
                {
                    return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                        "NodeType must be 'Departement', 'Navigation', or 'Category'.",
                        StatusCodes.Status400BadRequest);
                }

                var nodes = await _productNodeRepository.GetNodesByTypeAsync(nodeType);
                var response = nodes.Select(MapToGetProductNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                    $"An error occurred while retrieving nodes by type: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetProductNodeResponse>>> GetCategoryNodesAsync()
        {
            try
            {
                var nodes = await _productNodeRepository.GetCategoryNodesAsync();
                var response = nodes.Select(MapToGetProductNodeResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetProductNodeResponse>>(
                    $"An error occurred while retrieving category nodes: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateProductNodeResponse>> UpdateProductNodeAsync(UpdateProductNodeRequest request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdateProductNodeResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                var existingNode = await _productNodeRepository.GetNodeByIdAsync(request.Id);
                if (existingNode == null)
                {
                    return Result.Failure<UpdateProductNodeResponse>(
                        "Product node not found.",
                        StatusCodes.Status404NotFound);
                }

                // Validate parent exists if provided
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _productNodeRepository.ExistsAsync(request.ParentId.Value);
                    if (!parentExists)
                    {
                        return Result.Failure<UpdateProductNodeResponse>(
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

                var updatedNode = await _productNodeRepository.UpdateAsync(existingNode);

                var response = new UpdateProductNodeResponse
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
                return Result.Failure<UpdateProductNodeResponse>(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return Result.Failure<UpdateProductNodeResponse>(
                    $"An error occurred while updating the product node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteProductNodeResponse>> DeleteProductNodeAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<DeleteProductNodeResponse>(
                        "Product node ID is required.",
                        StatusCodes.Status400BadRequest);
                }

                var node = await _productNodeRepository.GetNodeByIdAsync(id);
                if (node == null)
                {
                    return Result.Failure<DeleteProductNodeResponse>(
                        "Product node not found.",
                        StatusCodes.Status404NotFound);
                }

                await _productNodeRepository.DeleteAsync(node);

                var response = new DeleteProductNodeResponse
                {
                    Id = id,
                    Success = true,
                    Message = "Product node deleted successfully."
                };

                return Result.Success(response);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<DeleteProductNodeResponse>(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return Result.Failure<DeleteProductNodeResponse>(
                    $"An error occurred while deleting the product node: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static GetProductNodeResponse MapToGetProductNodeResponse(BaseNode node)
        {
            return new GetProductNodeResponse
            {
                Id = node.Id,
                Name_en = node.Name_en,
                Name_fr = node.Name_fr,
                NodeType = node.NodeType,
                ParentId = node.ParentId,
                IsActive = node.IsActive,
                SortOrder = node.SortOrder,
                Children = new List<GetProductNodeResponse>()
            };
        }

        private static GetProductNodeResponse MapToGetProductNodeResponse(BaseNode node, IEnumerable<BaseNode> children)
        {
            return new GetProductNodeResponse
            {
                Id = node.Id,
                Name_en = node.Name_en,
                Name_fr = node.Name_fr,
                NodeType = node.NodeType,
                ParentId = node.ParentId,
                IsActive = node.IsActive,
                SortOrder = node.SortOrder,
                Children = children.Select(MapToGetProductNodeResponse).ToList()
            };
        }
    }
}
