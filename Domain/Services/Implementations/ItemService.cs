using System.Data;
using Dapper;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace Domain.Services.Implementations
{
    public class ItemService(
        IItemRepository itemRepository, 
        IItemVariantRepository itemVariantRepository,
        IItemAttributeRepository itemAttributeRepository,
        IItemVariantAttributeRepository itemVariantAttributeRepository,
        string connectionString) : IItemService
    {
        private readonly IItemRepository _itemRepository = itemRepository;
        private readonly IItemVariantRepository _itemVariantRepository = itemVariantRepository;
        private readonly IItemAttributeRepository _itemAttributeRepository = itemAttributeRepository;
        private readonly IItemVariantAttributeRepository _itemVariantAttributeRepository = itemVariantAttributeRepository;
        private readonly string _connectionString = connectionString;

        public async Task<Result<CreateItemResponse>> CreateItemAsync(CreateItemRequest createItemRequest)
        {
            try
            {
                var validationResult = createItemRequest.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateItemResponse>(validationResult.Error!, validationResult.ErrorCode ?? 400);
                }

                var itemId = Guid.NewGuid();
                var createdAt = DateTime.UtcNow;

                // Prepare Item
                var item = new Item
                {
                    Id = itemId,
                    SellerID = createItemRequest.SellerID,
                    Name_en = createItemRequest.Name_en,
                    Name_fr = createItemRequest.Name_fr,
                    Description_en = createItemRequest.Description_en,
                    Description_fr = createItemRequest.Description_fr,
                    CategoryID = createItemRequest.CategoryID,
                    CreatedAt = createdAt,
                    UpdatedAt = null,
                    Deleted = false
                };

                // Prepare ItemAttributes
                var itemAttributes = createItemRequest.ItemAttributes.Select(a => new ItemAttribute
                {
                    Id = Guid.NewGuid(),
                    ItemID = itemId,
                    AttributeName_en = a.AttributeName_en,
                    AttributeName_fr = a.AttributeName_fr,
                    Attributes_en = a.Attributes_en,
                    Attributes_fr = a.Attributes_fr
                }).ToList();

                // Prepare ItemVariants and ItemVariantAttributes
                var itemVariants = new List<ItemVariant>();
                var itemVariantAttributes = new List<ItemVariantAttribute>();

                foreach (var v in createItemRequest.Variants)
                {
                    var variantId = Guid.NewGuid();
                    var variant = new ItemVariant
                    {
                        Id = variantId,
                        ItemId = itemId,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity,
                        Sku = v.Sku,
                        ProductIdentifierType = v.ProductIdentifierType,
                        ProductIdentifierValue = v.ProductIdentifierValue,
                        ImageUrls = v.ImageUrls,
                        ThumbnailUrl = v.ThumbnailUrl,
                        ItemVariantName_en = v.ItemVariantName_en,
                        ItemVariantName_fr = v.ItemVariantName_fr,
                        Deleted = v.Deleted
                    };
                    itemVariants.Add(variant);

                    // Add variant attributes
                    foreach (var a in v.ItemVariantAttributes)
                    {
                        var variantAttribute = new ItemVariantAttribute
                        {
                            Id = Guid.NewGuid(),
                            ItemVariantID = variantId,
                            AttributeName_en = a.AttributeName_en,
                            AttributeName_fr = a.AttributeName_fr,
                            Attributes_en = a.Attributes_en,
                            Attributes_fr = a.Attributes_fr
                        };
                        itemVariantAttributes.Add(variantAttribute);
                    }
                }

                // Execute all database operations in a single transaction
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // 1. Insert Item
                    var itemQuery = @"
INSERT INTO dbo.Items (
    Id,
    SellerID,
    Name_en, 
    Name_fr, 
    Description_en, 
    Description_fr, 
    CategoryID, 
    CreatedAt, 
    UpdatedAt, 
    Deleted)
VALUES (
    @Id,
    @SellerID,
    @Name_en, 
    @Name_fr, 
    @Description_en, 
    @Description_fr, 
    @CategoryID, 
    @CreatedAt, 
    @UpdatedAt, 
    @Deleted)";

                    await connection.ExecuteAsync(itemQuery, item, transaction);

                    // 2. Insert ItemAttributes
                    if (itemAttributes.Any())
                    {
                        var itemAttributeQuery = @"
INSERT INTO dbo.ItemAttribute (Id, ItemID, AttributeName_en, AttributeName_fr, Attributes_en, Attributes_fr)
VALUES (@Id, @ItemID, @AttributeName_en, @AttributeName_fr, @Attributes_en, @Attributes_fr)";

                        foreach (var attribute in itemAttributes)
                        {
                            await connection.ExecuteAsync(itemAttributeQuery, attribute, transaction);
                        }
                    }

                    // 3. Insert ItemVariants
                    if (itemVariants.Any())
                    {
                        var itemVariantQuery = @"
INSERT INTO dbo.ItemVariants (
    Id,
    ItemId,
    Price,
    StockQuantity,
    Sku,
    ProductIdentifierType,
    ProductIdentifierValue,
    ImageUrls,
    ThumbnailUrl,
    ItemVariantName_en,
    ItemVariantName_fr,
    Deleted)
VALUES (
    @Id,
    @ItemId,
    @Price,
    @StockQuantity,
    @Sku,
    @ProductIdentifierType,
    @ProductIdentifierValue,
    @ImageUrls,
    @ThumbnailUrl,
    @ItemVariantName_en,
    @ItemVariantName_fr,
    @Deleted)";

                        foreach (var variant in itemVariants)
                        {
                            await connection.ExecuteAsync(itemVariantQuery, variant, transaction);
                        }
                    }

                    // 4. Insert ItemVariantAttributes
                    if (itemVariantAttributes.Any())
                    {
                        var itemVariantAttributeQuery = @"
INSERT INTO dbo.ItemVariantAttribute (Id, ItemVariantID, AttributeName_en, AttributeName_fr, Attributes_en, Attributes_fr)
VALUES (@Id, @ItemVariantID, @AttributeName_en, @AttributeName_fr, @Attributes_en, @Attributes_fr)";

                        foreach (var variantAttribute in itemVariantAttributes)
                        {
                            await connection.ExecuteAsync(itemVariantAttributeQuery, variantAttribute, transaction);
                        }
                    }

                    // Commit transaction - all operations succeeded
                    await transaction.CommitAsync();

                    // Set the collections on the item for the response
                    item.ItemAttributes = itemAttributes;
                    item.Variants = itemVariants;
                    
                    // Set ItemVariantAttributes on each variant
                    foreach (var variant in itemVariants)
                    {
                        variant.ItemVariantAttributes = itemVariantAttributes
                            .Where(va => va.ItemVariantID == variant.Id)
                            .ToList();
                    }

                    var response = new CreateItemResponse
                    {
                        Id = item.Id,
                        SellerID = item.SellerID,
                        Name_en = item.Name_en,
                        Name_fr = item.Name_fr,
                        Description_en = item.Description_en,
                        Description_fr = item.Description_fr,
                        CategoryID = item.CategoryID,
                        Variants = item.Variants,
                        ItemAttributes = item.ItemAttributes,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt,
                        Deleted = item.Deleted
                    };

                    return Result.Success(response);
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Transaction failed: {ex.Message}", ex);
                }
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
                    Description_en = item.Description_en,
                    Description_fr = item.Description_fr,
                    CategoryID = item.CategoryID,
                    Variants = item.Variants,
                    ItemAttributes = item.ItemAttributes,
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
                    Description_en = item.Description_en,
                    Description_fr = item.Description_fr,
                    CategoryID = item.CategoryID,
                    Variants = item.Variants,
                    ItemAttributes = item.ItemAttributes,
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
                existingItem.Description_en = updateItemRequest.Description_en;
                existingItem.Description_fr = updateItemRequest.Description_fr;
                existingItem.CategoryID = updateItemRequest.CategoryID;
                existingItem.Variants = updateItemRequest.Variants;
                existingItem.ItemAttributes = updateItemRequest.ItemAttributes;
                existingItem.UpdatedAt = DateTime.UtcNow;

                var updatedItem = await _itemRepository.UpdateAsync(existingItem);

                var response = new UpdateItemResponse
                {
                    Id = updatedItem.Id,
                    SellerID = updatedItem.SellerID,
                    Name_en = updatedItem.Name_en,
                    Name_fr = updatedItem.Name_fr,
                    Description_en = updatedItem.Description_en,
                    Description_fr = updatedItem.Description_fr,
                    CategoryID = updatedItem.CategoryID,
                    Variants = updatedItem.Variants,
                    ItemAttributes = updatedItem.ItemAttributes,
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
                var success = await _itemVariantRepository.DeleteItemVariantAsync(itemId, variantId);
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