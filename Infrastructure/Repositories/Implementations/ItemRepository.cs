using System.Data;
using System.Text.Json;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class ItemRepository(string connectionString) : GenericRepository<Item>(connectionString), IItemRepository
    {
        public override async Task<Item> AddAsync(Item entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            using var transaction = dbConnection.BeginTransaction();
            try
            {
                // Insert Item first
                var itemQuery = @"
INSERT INTO dbo.Items (
    SellerID,
    Name_en, 
    Name_fr, 
    Description_en, 
    Description_fr, 
    CategoryID, 
    CreatedAt, 
    UpdatedAt, 
    Deleted)
OUTPUT INSERTED.Id
VALUES (
    @SellerID,
    @Name_en, 
    @Name_fr, 
    @Description_en, 
    @Description_fr, 
    @CategoryID, 
    @CreatedAt, 
    @UpdatedAt, 
    @Deleted)";

                var itemParameters = new
                {
                    entity.SellerID,
                    entity.Name_en,
                    entity.Name_fr,
                    entity.Description_en,
                    entity.Description_fr,
                    entity.CategoryID,
                    entity.CreatedAt,
                    entity.UpdatedAt,
                    entity.Deleted
                };
                
                Guid newItemId = await dbConnection.ExecuteScalarAsync<Guid>(itemQuery, itemParameters, transaction);
                entity.Id = newItemId;

                // Insert variants if any
                if (entity.Variants?.Any() == true)
                {
                    var variantQuery = @"
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

                    foreach (var variant in entity.Variants)
                    {
                        variant.ItemId = newItemId;
                        if (variant.Id == Guid.Empty)
                        {
                            variant.Id = Guid.NewGuid();
                        }

                        var variantParameters = new
                        {
                            variant.Id,
                            variant.ItemId,
                            variant.Price,
                            variant.StockQuantity,
                            variant.Sku,
                            variant.ProductIdentifierType,
                            variant.ProductIdentifierValue,
                            variant.ImageUrls,
                            variant.ThumbnailUrl,
                            variant.ItemVariantName_en,
                            variant.ItemVariantName_fr,
                            variant.Deleted
                        };

                        await dbConnection.ExecuteAsync(variantQuery, variantParameters, transaction);
                    }
                }

                transaction.Commit();
                return entity;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public override async Task<int> CountAsync(Func<Item, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var items = await GetAllAsync();
            return items.Count(predicate);
        }

        public override async Task DeleteAsync(Item entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            entity.Deleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(entity);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Items WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<Item>> FindAsync(Func<Item, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var items = await GetAllAsync();
            return items.Where(predicate);
        }

        public override async Task<IEnumerable<Item>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT i.*, v.Id as VariantId, v.ItemId as VariantItemId, v.Price, v.StockQuantity, v.Sku, v.ProductIdentifierType, v.ProductIdentifierValue, v.ImageUrls, v.ThumbnailUrl, v.ItemVariantName_en, v.ItemVariantName_fr, v.Deleted as VariantDeleted
FROM dbo.Items i
LEFT JOIN dbo.ItemVariants v ON i.Id = v.ItemId AND v.Deleted = 0
WHERE i.Deleted = 0
ORDER BY i.Id";

            var itemDictionary = new Dictionary<Guid, Item>();
            
            await dbConnection.QueryAsync<ItemDto, ItemVariantDto, Item>(
                query,
                (itemDto, variantDto) =>
                {
                    if (!itemDictionary.TryGetValue(itemDto.Id, out var item))
                    {
                        item = MapToItem(itemDto);
                        itemDictionary.Add(itemDto.Id, item);
                    }

                    if (variantDto != null && variantDto.VariantId != Guid.Empty)
                    {
                        var variant = MapToItemVariant(variantDto);
                        item.Variants.Add(variant);
                    }

                    return item;
                },
                splitOn: "VariantId"
            );
            
            return itemDictionary.Values;
        }

        public override async Task<Item> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT i.*, v.Id as VariantId, v.ItemId as VariantItemId, v.Price, v.StockQuantity, v.Sku, v.ProductIdentifierType, v.ProductIdentifierValue, v.ImageUrls, v.ThumbnailUrl, v.ItemVariantName_en, v.ItemVariantName_fr, v.Deleted as VariantDeleted
FROM dbo.Items i
LEFT JOIN dbo.ItemVariants v ON i.Id = v.ItemId AND v.Deleted = 0
WHERE i.Id = @id
ORDER BY i.Id";

            Item? item = null;
            
            await dbConnection.QueryAsync<ItemDto, ItemVariantDto, Item>(
                query,
                (itemDto, variantDto) =>
                {
                    if (item == null)
                    {
                        item = MapToItem(itemDto);
                    }

                    if (variantDto != null && variantDto.VariantId != Guid.Empty)
                    {
                        var variant = MapToItemVariant(variantDto);
                        item.Variants.Add(variant);
                    }

                    return item;
                },
                new { id },
                splitOn: "VariantId"
            );
            
            return item ?? throw new InvalidOperationException($"Item with id {id} not found");
        }

        public override async Task<Item> UpdateAsync(Item entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            using var transaction = dbConnection.BeginTransaction();
            try
            {
                // Update Item
                var itemQuery = @"
UPDATE dbo.Items
SET
    SellerID = @SellerID,
    Name_en = @Name_en,
    Name_fr = @Name_fr,
    Description_en = @Description_en,
    Description_fr = @Description_fr,
    CategoryID = @CategoryID,
    UpdatedAt = @UpdatedAt,
    Deleted = @Deleted
WHERE Id = @Id";

                var itemParameters = new
                {
                    entity.Id,
                    entity.SellerID,
                    entity.Name_en,
                    entity.Name_fr,
                    entity.Description_en,
                    entity.Description_fr,
                    entity.CategoryID,
                    entity.UpdatedAt,
                    entity.Deleted
                };
                
                await dbConnection.ExecuteAsync(itemQuery, itemParameters, transaction);

                // Get existing variants to compare
                var existingVariantsQuery = "SELECT Id FROM dbo.ItemVariants WHERE ItemId = @ItemId";
                var existingVariantIds = await dbConnection.QueryAsync<Guid>(existingVariantsQuery, new { ItemId = entity.Id }, transaction);
                var existingVariantIdSet = existingVariantIds.ToHashSet();

                // Update or insert variants
                if (entity.Variants?.Any() == true)
                {
                    var variantUpsertQuery = @"
IF EXISTS (SELECT 1 FROM dbo.ItemVariants WHERE Id = @Id)
    UPDATE dbo.ItemVariants 
    SET Price = @Price, StockQuantity = @StockQuantity, 
        Sku = @Sku, ProductIdentifierType = @ProductIdentifierType, ProductIdentifierValue = @ProductIdentifierValue,
        ImageUrls = @ImageUrls, ThumbnailUrl = @ThumbnailUrl, ItemVariantName_en = @ItemVariantName_en, ItemVariantName_fr = @ItemVariantName_fr, Deleted = @Deleted
    WHERE Id = @Id
ELSE
    INSERT INTO dbo.ItemVariants (Id, ItemId, Price, StockQuantity, Sku, ProductIdentifierType, ProductIdentifierValue, ImageUrls, ThumbnailUrl, ItemVariantName_en, ItemVariantName_fr, Deleted)
    VALUES (@Id, @ItemId, @Price, @StockQuantity, @Sku, @ProductIdentifierType, @ProductIdentifierValue, @ImageUrls, @ThumbnailUrl, @ItemVariantName_en, @ItemVariantName_fr, @Deleted)";

                    var currentVariantIds = new HashSet<Guid>();

                    foreach (var variant in entity.Variants)
                    {
                        variant.ItemId = entity.Id;
                        if (variant.Id == Guid.Empty)
                        {
                            variant.Id = Guid.NewGuid();
                        }
                        
                        currentVariantIds.Add(variant.Id);

                        var variantParameters = new
                        {
                            variant.Id,
                            variant.ItemId,
                            variant.Price,
                            variant.StockQuantity,
                            variant.Sku,
                            variant.ProductIdentifierType,
                            variant.ProductIdentifierValue,
                            variant.ImageUrls,
                            variant.ThumbnailUrl,
                            variant.ItemVariantName_en,
                            variant.ItemVariantName_fr,
                            variant.Deleted
                        };

                        await dbConnection.ExecuteAsync(variantUpsertQuery, variantParameters, transaction);
                    }

                    // Mark variants as deleted if they're no longer in the entity but existed before
                    var variantsToDelete = existingVariantIdSet.Except(currentVariantIds);
                    if (variantsToDelete.Any())
                    {
                        var deleteQuery = "UPDATE dbo.ItemVariants SET Deleted = 1 WHERE Id IN @Ids";
                        await dbConnection.ExecuteAsync(deleteQuery, new { Ids = variantsToDelete }, transaction);
                    }
                }
                else
                {
                    // If no variants provided, mark all existing variants as deleted
                    if (existingVariantIdSet.Any())
                    {
                        var deleteAllQuery = "UPDATE dbo.ItemVariants SET Deleted = 1 WHERE ItemId = @ItemId";
                        await dbConnection.ExecuteAsync(deleteAllQuery, new { ItemId = entity.Id }, transaction);
                    }
                }

                transaction.Commit();
                return entity;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // IItemRepository specific methods
        public async Task<Item?> GetItemByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT i.*, v.Id as VariantId, v.ItemId as VariantItemId, v.Price, v.StockQuantity, v.Sku, v.ProductIdentifierType, v.ProductIdentifierValue, v.ImageUrls, v.ThumbnailUrl, v.ItemVariantName_en, v.ItemVariantName_fr, v.Deleted as VariantDeleted
FROM dbo.Items i
LEFT JOIN dbo.ItemVariants v ON i.Id = v.ItemId AND v.Deleted = 0
WHERE i.Id = @id AND i.Deleted = 0
ORDER BY i.Id";

            Item? item = null;
            
            await dbConnection.QueryAsync<ItemDto, ItemVariantDto, Item>(
                query,
                (itemDto, variantDto) =>
                {
                    if (item == null)
                    {
                        item = MapToItem(itemDto);
                    }

                    if (variantDto != null && variantDto.VariantId != Guid.Empty)
                    {
                        var variant = MapToItemVariant(variantDto);
                        item.Variants.Add(variant);
                    }

                    return item;
                },
                new { id },
                splitOn: "VariantId"
            );
            
            return item;
        }

        public async Task<bool> DeleteItemVariantAsync(Guid itemId, Guid variantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.ItemVariants 
SET Deleted = 1 
WHERE Id = @variantId AND ItemId = @itemId AND Deleted = 0";
            
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { variantId, itemId });
            
            if (rowsAffected > 0)
            {
                // Update the item's UpdatedAt timestamp
                var updateItemQuery = "UPDATE dbo.Items SET UpdatedAt = @UpdatedAt WHERE Id = @itemId";
                await dbConnection.ExecuteAsync(updateItemQuery, new { UpdatedAt = DateTime.UtcNow, itemId });
                return true;
            }
            
            return false;
        }

        private static Item MapToItem(ItemDto dto)
        {
            return new Item
            {
                Id = dto.Id,
                SellerID = dto.SellerID,
                Name_en = dto.Name_en,
                Name_fr = dto.Name_fr,
                Description_en = dto.Description_en,
                Description_fr = dto.Description_fr,
                CategoryID = dto.CategoryID,
                Variants = new List<ItemVariant>(), // Variants will be added separately
                ItemAttributes = new List<ItemAttribute>(), // Will be populated separately when needed
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                Deleted = dto.Deleted
            };
        }

        private static ItemVariant MapToItemVariant(ItemVariantDto dto)
        {
            return new ItemVariant
            {
                Id = dto.VariantId,
                ItemId = dto.VariantItemId,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Sku = dto.Sku ?? string.Empty,
                ProductIdentifierType = dto.ProductIdentifierType,
                ProductIdentifierValue = dto.ProductIdentifierValue,
                ImageUrls = dto.ImageUrls,
                ThumbnailUrl = dto.ThumbnailUrl,
                ItemVariantName_en = dto.ItemVariantName_en,
                ItemVariantName_fr = dto.ItemVariantName_fr,
                ItemVariantAttributes = new List<ItemVariantAttribute>(), // Will be populated separately when needed
                Deleted = dto.VariantDeleted
            };
        }

        private sealed class ItemDto
        {
            public Guid Id { get; set; }
            public Guid SellerID { get; set; }
            public string Name_en { get; set; } = string.Empty;
            public string? Name_fr { get; set; }
            public string? Description_en { get; set; }
            public string? Description_fr { get; set; }
            public Guid CategoryID { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public bool Deleted { get; set; }
        }

        private sealed class ItemVariantDto
        {
            public Guid VariantId { get; set; }
            public Guid VariantItemId { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string? Sku { get; set; }
            public string? ProductIdentifierType { get; set; }
            public string? ProductIdentifierValue { get; set; }
            public string? ImageUrls { get; set; }
            public string? ThumbnailUrl { get; set; }
            public string? ItemVariantName_en { get; set; }
            public string? ItemVariantName_fr { get; set; }
            public bool VariantDeleted { get; set; }
        }
    }
}