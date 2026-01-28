using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface ICategoryNodeService
    {
        Task<Result<CreateCategoryNodeResponse>> CreateCategoryNodeAsync(CreateCategoryNodeRequest request);
        Task<Result<BulkCreateCategoryNodesResponse>> CreateStructureAsync(BulkCreateCategoryNodesRequest request);
        Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetAllCategoryNodesAsync();
        Task<Result<GetCategoryNodeResponse>> GetCategoryNodeByIdAsync(Guid id);
        Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetRootNodesAsync();
        Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetChildrenAsync(Guid parentId);
        Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetNodesByTypeAsync(string nodeType);
        Task<Result<IEnumerable<GetCategoryNodeResponse>>> GetCategoryNodesAsync();
        Task<Result<UpdateCategoryNodeResponse>> UpdateCategoryNodeAsync(UpdateCategoryNodeRequest request);
        Task<Result<DeleteCategoryNodeResponse>> DeleteCategoryNodeAsync(Guid id);
    }
}
