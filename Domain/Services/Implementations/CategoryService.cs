using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;

        public async Task<Result<CreateCategoryResponse>> CreateCategoryAsync(CreateCategoryRequest createCategoryRequest)
        {
            try
            {
                var validationResult = createCategoryRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateCategoryResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                // Validate parent category exists if provided
                if (createCategoryRequest.ParentCategoryId.HasValue)
                {
                    var parentExists = await _categoryRepository.ExistsAsync(createCategoryRequest.ParentCategoryId.Value);
                    if (!parentExists)
                    {
                        return Result.Failure<CreateCategoryResponse>(
                            "Parent category does not exist.", 
                            StatusCodes.Status400BadRequest);
                    }
                }

                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name_en = createCategoryRequest.Name_en,
                    Name_fr = createCategoryRequest.Name_fr,
                    ParentCategoryId = createCategoryRequest.ParentCategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdCategory = await _categoryRepository.AddAsync(category);

                var response = new CreateCategoryResponse
                {
                    Id = createdCategory.Id,
                    Name_en = createdCategory.Name_en,
                    Name_fr = createdCategory.Name_fr,
                    ParentCategoryId = createdCategory.ParentCategoryId
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateCategoryResponse>(
                    $"An error occurred while creating the category: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCategoryResponse>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var response = categories.Select(MapToGetCategoryResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryResponse>>(
                    $"An error occurred while retrieving categories: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetCategoryResponse>> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<GetCategoryResponse>(
                        "Category ID is required.",
                        StatusCodes.Status400BadRequest);
                }

                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return Result.Failure<GetCategoryResponse>(
                        "Category not found.",
                        StatusCodes.Status404NotFound);
                }

                // Get subcategories
                var subcategories = await _categoryRepository.GetSubcategoriesAsync(id);

                var response = MapToGetCategoryResponse(category, subcategories);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<GetCategoryResponse>(
                    $"An error occurred while retrieving the category: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCategoryResponse>>> GetRootCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetRootCategoriesAsync();
                var response = categories.Select(MapToGetCategoryResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryResponse>>(
                    $"An error occurred while retrieving root categories: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetCategoryResponse>>> GetSubcategoriesAsync(Guid parentCategoryId)
        {
            try
            {
                if (parentCategoryId == Guid.Empty)
                {
                    return Result.Failure<IEnumerable<GetCategoryResponse>>(
                        "Parent category ID is required.",
                        StatusCodes.Status400BadRequest);
                }

                var parentExists = await _categoryRepository.ExistsAsync(parentCategoryId);
                if (!parentExists)
                {
                    return Result.Failure<IEnumerable<GetCategoryResponse>>(
                        "Parent category does not exist.",
                        StatusCodes.Status404NotFound);
                }

                var categories = await _categoryRepository.GetSubcategoriesAsync(parentCategoryId);
                var response = categories.Select(MapToGetCategoryResponse);
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetCategoryResponse>>(
                    $"An error occurred while retrieving subcategories: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateCategoryResponse>> UpdateCategoryAsync(UpdateCategoryRequest updateCategoryRequest)
        {
            try
            {
                var validationResult = updateCategoryRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdateCategoryResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                var existingCategory = await _categoryRepository.GetCategoryByIdAsync(updateCategoryRequest.Id);
                if (existingCategory == null)
                {
                    return Result.Failure<UpdateCategoryResponse>(
                        "Category not found.",
                        StatusCodes.Status404NotFound);
                }

                // Validate parent category exists if provided
                if (updateCategoryRequest.ParentCategoryId.HasValue)
                {
                    var parentExists = await _categoryRepository.ExistsAsync(updateCategoryRequest.ParentCategoryId.Value);
                    if (!parentExists)
                    {
                        return Result.Failure<UpdateCategoryResponse>(
                            "Parent category does not exist.",
                            StatusCodes.Status400BadRequest);
                    }
                }

                existingCategory.Name_en = updateCategoryRequest.Name_en;
                existingCategory.Name_fr = updateCategoryRequest.Name_fr;
                existingCategory.ParentCategoryId = updateCategoryRequest.ParentCategoryId;
                existingCategory.UpdatedAt = DateTime.UtcNow;

                var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);

                var response = new UpdateCategoryResponse
                {
                    Id = updatedCategory.Id,
                    Name_en = updatedCategory.Name_en,
                    Name_fr = updatedCategory.Name_fr,
                    ParentCategoryId = updatedCategory.ParentCategoryId
                };

                return Result.Success(response);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<UpdateCategoryResponse>(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return Result.Failure<UpdateCategoryResponse>(
                    $"An error occurred while updating the category: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteCategoryResponse>> DeleteCategoryAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<DeleteCategoryResponse>(
                        "Category ID is required.",
                        StatusCodes.Status400BadRequest);
                }

                var category = await _categoryRepository.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return Result.Failure<DeleteCategoryResponse>(
                        "Category not found.",
                        StatusCodes.Status404NotFound);
                }

                await _categoryRepository.DeleteAsync(category);

                var response = new DeleteCategoryResponse
                {
                    Id = id,
                    Success = true,
                    Message = "Category deleted successfully."
                };

                return Result.Success(response);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<DeleteCategoryResponse>(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                return Result.Failure<DeleteCategoryResponse>(
                    $"An error occurred while deleting the category: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static GetCategoryResponse MapToGetCategoryResponse(Category category)
        {
            return new GetCategoryResponse
            {
                Id = category.Id,
                Name_en = category.Name_en,
                Name_fr = category.Name_fr,
                ParentCategoryId = category.ParentCategoryId,
                Subcategories = new List<GetCategoryResponse>()
            };
        }

        private static GetCategoryResponse MapToGetCategoryResponse(Category category, IEnumerable<Category> subcategories)
        {
            return new GetCategoryResponse
            {
                Id = category.Id,
                Name_en = category.Name_en,
                Name_fr = category.Name_fr,
                ParentCategoryId = category.ParentCategoryId,
                Subcategories = subcategories.Select(MapToGetCategoryResponse).ToList()
            };
        }
    }
}