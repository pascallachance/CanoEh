using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<CreateCategoryResponse>> CreateCategoryAsync(CreateCategoryRequest createCategoryRequest);
        Task<Result<IEnumerable<GetCategoryResponse>>> GetAllCategoriesAsync();
        Task<Result<GetCategoryResponse>> GetCategoryByIdAsync(Guid id);
        Task<Result<IEnumerable<GetCategoryResponse>>> GetRootCategoriesAsync();
        Task<Result<IEnumerable<GetCategoryResponse>>> GetSubcategoriesAsync(Guid parentCategoryId);
        Task<Result<UpdateCategoryResponse>> UpdateCategoryAsync(UpdateCategoryRequest updateCategoryRequest);
        Task<Result<DeleteCategoryResponse>> DeleteCategoryAsync(Guid id);
    }
}