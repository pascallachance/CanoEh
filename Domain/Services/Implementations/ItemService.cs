using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class ItemService(IItemRepository itemRepository) : IItemService
    {
        private readonly IItemRepository _itemRepository = itemRepository;

        public async Task<Result<CreateItemResponse>> CreateItemAsync(CreateItemRequest createItemRequest)
        {
            try
            {
                var validationResult = createItemRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateItemResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                var item = new Item
                {
                    Id = Guid.NewGuid(),
                    SellerID = createItemRequest.SellerID,
                    Name_en = createItemRequest.Name_en,
                    Name_fr = createItemRequest.Name_fr,
                    Description = createItemRequest.Description,
                    Brand = createItemRequest.Brand,
                    Category = createItemRequest.Category,
                    Variants = createItemRequest.Variants,
                    ImageUrls = createItemRequest.ImageUrls,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null,
                    Deleted = false
                };

                var createdItem = await _itemRepository.AddAsync(item);

                var response = new CreateItemResponse
                {
                    Id = createdItem.Id,
                    SellerID = createdItem.SellerID,
                    Name_en = createdItem.Name_en,
                    Name_fr = createdItem.Name_fr,
                    Description = createdItem.Description,
                    Brand = createdItem.Brand,
                    Category = createdItem.Category,
                    Variants = createdItem.Variants,
                    ImageUrls = createdItem.ImageUrls,
                    CreatedAt = createdItem.CreatedAt,
                    UpdatedAt = createdItem.UpdatedAt,
                    Deleted = createdItem.Deleted
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<CreateItemResponse>($"An error occurred while creating the item: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetItemResponse>>> GetAllItemsAsync()
        {
            try
            {
                var items = await _itemRepository.GetAllAsync();
                var response = items.Select(item => new GetItemResponse
                {
                    Id = item.Id,
                    SellerID = item.SellerID,
                    Name_en = item.Name_en,
                    Name_fr = item.Name_fr,
                    Description = item.Description,
                    Brand = item.Brand,
                    Category = item.Category,
                    Variants = item.Variants,
                    ImageUrls = item.ImageUrls,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    Deleted = item.Deleted
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetItemResponse>>($"An error occurred while retrieving items: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<GetItemResponse>> GetItemByIdAsync(Guid id)
        {
            try
            {
                var item = await _itemRepository.GetItemByIdAsync(id);
                if (item == null)
                {
                    return Result.Failure<GetItemResponse>("Item not found.", StatusCodes.Status404NotFound);
                }

                var response = new GetItemResponse
                {
                    Id = item.Id,
                    SellerID = item.SellerID,
                    Name_en = item.Name_en,
                    Name_fr = item.Name_fr,
                    Description = item.Description,
                    Brand = item.Brand,
                    Category = item.Category,
                    Variants = item.Variants,
                    ImageUrls = item.ImageUrls,
                    CreatedAt = item.CreatedAt,
                    UpdatedAt = item.UpdatedAt,
                    Deleted = item.Deleted
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<GetItemResponse>($"An error occurred while retrieving the item: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<UpdateItemResponse>> UpdateItemAsync(UpdateItemRequest updateItemRequest)
        {
            try
            {
                var validationResult = updateItemRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<UpdateItemResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                var existingItem = await _itemRepository.GetItemByIdAsync(updateItemRequest.Id);
                if (existingItem == null)
                {
                    return Result.Failure<UpdateItemResponse>("Item not found.", StatusCodes.Status404NotFound);
                }

                existingItem.SellerID = updateItemRequest.SellerID;
                existingItem.Name_en = updateItemRequest.Name_en;
                existingItem.Name_fr = updateItemRequest.Name_fr;
                existingItem.Description = updateItemRequest.Description;
                existingItem.Brand = updateItemRequest.Brand;
                existingItem.Category = updateItemRequest.Category;
                existingItem.Variants = updateItemRequest.Variants;
                existingItem.ImageUrls = updateItemRequest.ImageUrls;
                existingItem.UpdatedAt = DateTime.UtcNow;

                var updatedItem = await _itemRepository.UpdateAsync(existingItem);

                var response = new UpdateItemResponse
                {
                    Id = updatedItem.Id,
                    SellerID = updatedItem.SellerID,
                    Name_en = updatedItem.Name_en,
                    Name_fr = updatedItem.Name_fr,
                    Description = updatedItem.Description,
                    Brand = updatedItem.Brand,
                    Category = updatedItem.Category,
                    Variants = updatedItem.Variants,
                    ImageUrls = updatedItem.ImageUrls,
                    CreatedAt = updatedItem.CreatedAt,
                    UpdatedAt = updatedItem.UpdatedAt,
                    Deleted = updatedItem.Deleted
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<UpdateItemResponse>($"An error occurred while updating the item: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteItemResponse>> DeleteItemAsync(Guid id)
        {
            try
            {
                var item = await _itemRepository.GetItemByIdAsync(id);
                if (item == null)
                {
                    return Result.Failure<DeleteItemResponse>("Item not found.", StatusCodes.Status404NotFound);
                }

                await _itemRepository.DeleteAsync(item);

                var response = new DeleteItemResponse
                {
                    Id = id,
                    Message = "Item deleted successfully.",
                    Success = true
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<DeleteItemResponse>($"An error occurred while deleting the item: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<DeleteItemVariantResponse>> DeleteItemVariantAsync(Guid itemId, Guid variantId)
        {
            try
            {
                var success = await _itemRepository.DeleteItemVariantAsync(itemId, variantId);
                if (!success)
                {
                    return Result.Failure<DeleteItemVariantResponse>("Item or variant not found.", StatusCodes.Status404NotFound);
                }

                var response = new DeleteItemVariantResponse
                {
                    ItemId = itemId,
                    VariantId = variantId,
                    Message = "Item variant deleted successfully.",
                    Success = true
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<DeleteItemVariantResponse>($"An error occurred while deleting the item variant: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }
    }
}