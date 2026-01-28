using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface IProductNodeService
    {
        Task<Result<CreateProductNodeResponse>> CreateProductNodeAsync(CreateProductNodeRequest request);
        Task<Result<IEnumerable<GetProductNodeResponse>>> GetAllProductNodesAsync();
        Task<Result<GetProductNodeResponse>> GetProductNodeByIdAsync(Guid id);
        Task<Result<IEnumerable<GetProductNodeResponse>>> GetRootNodesAsync();
        Task<Result<IEnumerable<GetProductNodeResponse>>> GetChildrenAsync(Guid parentId);
        Task<Result<IEnumerable<GetProductNodeResponse>>> GetNodesByTypeAsync(string nodeType);
        Task<Result<IEnumerable<GetProductNodeResponse>>> GetCategoryNodesAsync();
        Task<Result<UpdateProductNodeResponse>> UpdateProductNodeAsync(UpdateProductNodeRequest request);
        Task<Result<DeleteProductNodeResponse>> DeleteProductNodeAsync(Guid id);
    }
}
