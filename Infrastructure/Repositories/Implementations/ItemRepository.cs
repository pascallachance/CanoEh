using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

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
            
            // Insert Item only - variants should be managed by ItemVariantRepository
            var itemQuery = @"
INSERT INTO dbo.Item (
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
            
            Guid newItemId = await dbConnection.ExecuteScalarAsync<Guid>(itemQuery, itemParameters);
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
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Item WHERE Id = @id", new { id });
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
            
            // Query Item table only - variants should be loaded separately via ItemVariantRepository
            var query = "SELECT * FROM dbo.Item WHERE Deleted = 0";
            
            return await dbConnection.QueryAsync<Item>(query);
        }

        public override async Task<Item> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Query Item table only - variants should be loaded separately via ItemVariantRepository
            var query = "SELECT * FROM dbo.Item WHERE Id = @id";

            var item = await dbConnection.QueryFirstOrDefaultAsync<Item>(query, new { id });
            
            return item ?? throw new InvalidOperationException($"Item with id {id} not found");
        }

        public override async Task<Item> UpdateAsync(Item entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Update Item only - variants should be managed by ItemVariantRepository
            var itemQuery = @"
UPDATE dbo.Item
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
            
            await dbConnection.ExecuteAsync(itemQuery, itemParameters);

            return entity;
        }

        // IItemRepository specific methods
        public async Task<Item?> GetItemByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Query Item table only - variants should be loaded separately via ItemVariantRepository
            var query = "SELECT * FROM dbo.Item WHERE Id = @id AND Deleted = 0";

            return await dbConnection.QueryFirstOrDefaultAsync<Item>(query, new { id });
        }

        public async Task<IEnumerable<Item>> GetBySellerIdAsync(Guid sellerId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Get all items for the seller (including deleted items)
            var itemQuery = "SELECT * FROM dbo.Item WHERE SellerID = @sellerId";
            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { sellerId })).ToList();
            
            if (!items.Any())
            {
                return items;
            }
            
            var itemIds = items.Select(i => i.Id).ToList();
            
            // Get all ItemAttributes for the items and group by ItemID for O(1) lookup
            var itemAttributeQuery = "SELECT * FROM dbo.ItemAttribute WHERE ItemID IN @itemIds";
            var itemAttributes = (await dbConnection.QueryAsync<ItemAttribute>(itemAttributeQuery, new { itemIds })).ToList();
            var itemAttributesByItemId = itemAttributes.GroupBy(ia => ia.ItemID).ToDictionary(g => g.Key, g => g.ToList());
            
            // Get all ItemVariants for the items (including deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());
            
            if (variants.Any())
            {
                var variantIds = variants.Select(v => v.Id).ToList();
                
                // Get all ItemVariantAttributes for the variants and group by ItemVariantID for O(1) lookup
                var variantAttributeQuery = "SELECT * FROM dbo.ItemVariantAttribute WHERE ItemVariantID IN @variantIds";
                var variantAttributes = (await dbConnection.QueryAsync<ItemVariantAttribute>(variantAttributeQuery, new { variantIds })).ToList();
                var variantAttributesByVariantId = variantAttributes.GroupBy(va => va.ItemVariantID).ToDictionary(g => g.Key, g => g.ToList());
                
                // Assign ItemVariantAttributes to their respective ItemVariants using dictionary lookup
                foreach (var variant in variants)
                {
                    variant.ItemVariantAttributes = variantAttributesByVariantId.TryGetValue(variant.Id, out var attrs) ? attrs : new List<ItemVariantAttribute>();
                }
            }
            
            // Assign ItemVariants and ItemAttributes to their respective Items using dictionary lookup
            foreach (var item in items)
            {
                item.Variants = variantsByItemId.TryGetValue(item.Id, out var vars) ? vars : new List<ItemVariant>();
                item.ItemAttributes = itemAttributesByItemId.TryGetValue(item.Id, out var attrs) ? attrs : new List<ItemAttribute>();
            }
            
            return items;
        }
    }
}