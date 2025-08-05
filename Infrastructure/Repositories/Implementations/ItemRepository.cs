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
            
            var query = @"
INSERT INTO dbo.Items (
    SellerID,
    Name, 
    Description, 
    Brand, 
    Category, 
    Variants,
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
    @Variants,
    @ImageUrls,
    @CreatedAt, 
    @UpdatedAt, 
    @Deleted)";

            var parameters = new
            {
                entity.SellerID,
                entity.Name,
                entity.Description,
                entity.Brand,
                entity.Category,
                Variants = JsonSerializer.Serialize(entity.Variants),
                ImageUrls = JsonSerializer.Serialize(entity.ImageUrls),
                entity.CreatedAt,
                entity.UpdatedAt,
                entity.Deleted
            };
            
            Guid newItemId = await dbConnection.ExecuteScalarAsync<Guid>(query, parameters);
            entity.Id = newItemId;
            return entity;
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
            
            var query = "SELECT * FROM dbo.Items WHERE Deleted = 0";
            var itemDtos = await dbConnection.QueryAsync<ItemDto>(query);
            
            return itemDtos.Select(MapToItem);
        }

        public override async Task<Item> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT TOP(1) * 
FROM dbo.Items 
WHERE Id = @id";
            
            var itemDto = await dbConnection.QueryFirstAsync<ItemDto>(query, new { id });
            return MapToItem(itemDto);
        }

        public override async Task<Item> UpdateAsync(Item entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.Items
SET
    SellerID = @SellerID,
    Name = @Name,
    Description = @Description,
    Brand = @Brand,
    Category = @Category,
    Variants = @Variants,
    ImageUrls = @ImageUrls,
    UpdatedAt = @UpdatedAt,
    Deleted = @Deleted
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.SellerID,
                entity.Name,
                entity.Description,
                entity.Brand,
                entity.Category,
                Variants = JsonSerializer.Serialize(entity.Variants),
                ImageUrls = JsonSerializer.Serialize(entity.ImageUrls),
                entity.UpdatedAt,
                entity.Deleted
            };
            
            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        // IItemRepository specific methods
        public async Task<Item?> GetItemByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT TOP(1) * 
FROM dbo.Items 
WHERE Id = @id AND Deleted = 0";
            
            var itemDto = await dbConnection.QueryFirstOrDefaultAsync<ItemDto>(query, new { id });
            return itemDto != null ? MapToItem(itemDto) : null;
        }

        public async Task<bool> DeleteItemVariantAsync(Guid itemId, Guid variantId)
        {
            var item = await GetItemByIdAsync(itemId);
            if (item == null) return false;
            
            var variant = item.Variants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null) return false;
            
            variant.Deleted = true;
            item.UpdatedAt = DateTime.UtcNow;
            
            await UpdateAsync(item);
            return true;
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
                Variants = string.IsNullOrEmpty(dto.Variants) 
                    ? new List<ItemVariant>() 
                    : JsonSerializer.Deserialize<List<ItemVariant>>(dto.Variants) ?? new List<ItemVariant>(),
                ImageUrls = string.IsNullOrEmpty(dto.ImageUrls) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(dto.ImageUrls) ?? new List<string>(),
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                Deleted = dto.Deleted
            };
        }

        private class ItemDto
        {
            public Guid Id { get; set; }
            public Guid SellerID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Brand { get; set; }
            public string? Category { get; set; }
            public string? Variants { get; set; }
            public string? ImageUrls { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public bool Deleted { get; set; }
        }
    }
}