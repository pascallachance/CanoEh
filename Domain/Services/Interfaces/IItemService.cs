using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;
using Infrastructure.Data;

namespace Domain.Services.Interfaces
{
    public interface IItemService
    {
        Task<Result<CreateItemResponse>> CreateItemAsync(CreateItemRequest createItemRequest);
        Task<Result<IEnumerable<GetItemResponse>>> GetAllItemsAsync();
        Task<Result<GetItemResponse>> GetItemByIdAsync(Guid id);
        Task<Result<UpdateItemResponse>> UpdateItemAsync(UpdateItemRequest updateItemRequest);
        Task<Result<DeleteItemResponse>> DeleteItemAsync(Guid id);
        Task<Result<DeleteItemVariantResponse>> DeleteItemVariantAsync(Guid itemId, Guid variantId);
    }
}