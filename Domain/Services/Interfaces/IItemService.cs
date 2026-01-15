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
        Task<Result<GetItemResponse>> GetItemByIdIncludingDeletedAsync(Guid id);
        Task<Result<IEnumerable<GetItemResponse>>> GetAllItemsFromSellerAsync(Guid sellerId, bool includeDeleted = false);
        Task<Result<UpdateItemResponse>> UpdateItemAsync(UpdateItemRequest updateItemRequest);
        Task<Result<DeleteItemResponse>> DeleteItemAsync(Guid id);
        Task<Result<DeleteItemVariantResponse>> DeleteItemVariantAsync(Guid itemId, Guid variantId);
        Task<Result<DeleteItemResponse>> UnDeleteItemAsync(Guid id);
        Task<Result<DeleteItemVariantResponse>> UnDeleteItemVariantAsync(Guid itemId, Guid variantId);
        Task<Result<GetItemResponse>> GetItemByVariantIdAsync(Guid variantId, Guid userId);
        Task<Result> UpdateItemVariantAsync(ItemVariant variant);
        Task<Result> UpdateItemVariantImageAsync(Guid variantId, string imageType, string imageUrl, int imageNumber);
        Task<Result<IEnumerable<GetItemResponse>>> GetRecentlyAddedProductsAsync(int count = 100);
        Task<Result<IEnumerable<GetItemResponse>>> GetSuggestedProductsAsync(int count = 4);
        Task<Result<IEnumerable<GetItemResponse>>> GetProductsWithOffersAsync(int count = 10);
        Task<Result> UpdateItemVariantOfferAsync(UpdateItemVariantOfferRequest request);
        Task<Result> BatchUpdateItemVariantOffersAsync(BatchUpdateItemVariantOffersRequest request, Guid userId);
    }
}