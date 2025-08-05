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
    Name, 
    Description, 
    Brand, 
    Category, 
    ImageUrls,
    CreatedAt, 
    UpdatedAt, 
    Deleted)
OUTPUT INSERTED.Id
VALUES (
    @SellerID,
    @Name, 
    @Description, 
    @Brand, 
    @Category, 
    @ImageUrls,
    @CreatedAt, 
    @UpdatedAt, 
    @Deleted)";

                var itemParameters = new
                {
                    entity.SellerID,
                    entity.Name,
                    entity.Description,
                    entity.Brand,
                    entity.Category,
                    ImageUrls = JsonSerializer.Serialize(entity.ImageUrls),
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
    Attributes,
    Price,
    StockQuantity,
    Sku,
    ThumbnailUrls,
    Deleted)
VALUES (
    @Id,
    @ItemId,
    @Attributes,
    @Price,
    @StockQuantity,
    @Sku,
    @ThumbnailUrls,
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
                            Attributes = JsonSerializer.Serialize(variant.Attributes),
                            variant.Price,
                            variant.StockQuantity,
                            variant.Sku,
                            ThumbnailUrls = JsonSerializer.Serialize(variant.ThumbnailUrls),
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
SELECT i.*, v.Id as VariantId, v.ItemId as VariantItemId, v.Attributes, v.Price, v.StockQuantity, v.Sku, v.ThumbnailUrls, v.Deleted as VariantDeleted
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
SELECT i.*, v.Id as VariantId, v.ItemId as VariantItemId, v.Attributes, v.Price, v.StockQuantity, v.Sku, v.ThumbnailUrls, v.Deleted as VariantDeleted
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
    Name = @Name,
    Description = @Description,
    Brand = @Brand,
    Category = @Category,
    ImageUrls = @ImageUrls,
    UpdatedAt = @UpdatedAt,
    Deleted = @Deleted
WHERE Id = @Id";

                var itemParameters = new
                {
                    entity.Id,
                    entity.SellerID,
                    entity.Name,
                    entity.Description,
                    entity.Brand,
                    entity.Category,
                    ImageUrls = JsonSerializer.Serialize(entity.ImageUrls),
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
    SET Attributes = @Attributes, Price = @Price, StockQuantity = @StockQuantity, 
        Sku = @Sku, ThumbnailUrls = @ThumbnailUrls, Deleted = @Deleted
    WHERE Id = @Id
ELSE
    INSERT INTO dbo.ItemVariants (Id, ItemId, Attributes, Price, StockQuantity, Sku, ThumbnailUrls, Deleted)
    VALUES (@Id, @ItemId, @Attributes, @Price, @StockQuantity, @Sku, @ThumbnailUrls, @Deleted)";

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
                            Attributes = JsonSerializer.Serialize(variant.Attributes),
                            variant.Price,
                            variant.StockQuantity,
                            variant.Sku,
                            ThumbnailUrls = JsonSerializer.Serialize(variant.ThumbnailUrls),
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
SELECT i.*, v.Id as VariantId, v.ItemId as VariantItemId, v.Attributes, v.Price, v.StockQuantity, v.Sku, v.ThumbnailUrls, v.Deleted as VariantDeleted
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
                Name = dto.Name,
                Description = dto.Description,
                Brand = dto.Brand,
                Category = dto.Category,
                Variants = new List<ItemVariant>(), // Variants will be added separately
                ImageUrls = string.IsNullOrEmpty(dto.ImageUrls) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(dto.ImageUrls) ?? new List<string>(),
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
                Attributes = string.IsNullOrEmpty(dto.Attributes)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(dto.Attributes) ?? new Dictionary<string, string>(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Sku = dto.Sku,
                ThumbnailUrls = string.IsNullOrEmpty(dto.ThumbnailUrls)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(dto.ThumbnailUrls) ?? new List<string>(),
                Deleted = dto.VariantDeleted
            };
        }

        private sealed class ItemDto
        {
            public Guid Id { get; set; }
            public Guid SellerID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Brand { get; set; }
            public string? Category { get; set; }
            public string? ImageUrls { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public bool Deleted { get; set; }
        }

        private sealed class ItemVariantDto
        {
            public Guid VariantId { get; set; }
            public Guid VariantItemId { get; set; }
            public string? Attributes { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string? Sku { get; set; }
            public string? ThumbnailUrls { get; set; }
            public bool VariantDeleted { get; set; }
        }
    }
}