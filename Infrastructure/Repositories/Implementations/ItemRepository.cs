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
    }
}