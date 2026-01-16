using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class ItemVariantRepository(string connectionString) : GenericRepository<ItemVariant>(connectionString), IItemVariantRepository
    {
        public override async Task<ItemVariant> AddAsync(ItemVariant entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            var query = @"
INSERT INTO dbo.ItemVariant (
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

            var parameters = new
            {
                entity.Id,
                entity.ItemId,
                entity.Price,
                entity.StockQuantity,
                entity.Sku,
                entity.ProductIdentifierType,
                entity.ProductIdentifierValue,
                entity.ImageUrls,
                entity.ThumbnailUrl,
                entity.ItemVariantName_en,
                entity.ItemVariantName_fr,
                entity.Deleted
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override async Task<int> CountAsync(Func<ItemVariant, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Count(predicate);
        }

        public override async Task DeleteAsync(ItemVariant entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            entity.Deleted = true;
            await UpdateAsync(entity);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.ItemVariant WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<ItemVariant>> FindAsync(Func<ItemVariant, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Where(predicate);
        }

        public override async Task<IEnumerable<ItemVariant>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariant WHERE Deleted = 0";
            return await dbConnection.QueryAsync<ItemVariant>(query);
        }

        public override async Task<ItemVariant> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariant WHERE Id = @id";
            var result = await dbConnection.QueryFirstOrDefaultAsync<ItemVariant>(query, new { id });
            
            return result ?? throw new InvalidOperationException($"ItemVariant with id {id} not found");
        }

        public override async Task<ItemVariant> UpdateAsync(ItemVariant entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.ItemVariant 
SET ItemId = @ItemId,
    Price = @Price,
    StockQuantity = @StockQuantity,
    Sku = @Sku,
    ProductIdentifierType = @ProductIdentifierType,
    ProductIdentifierValue = @ProductIdentifierValue,
    ImageUrls = @ImageUrls,
    ThumbnailUrl = @ThumbnailUrl,
    ItemVariantName_en = @ItemVariantName_en,
    ItemVariantName_fr = @ItemVariantName_fr,
    Deleted = @Deleted,
    Offer = @Offer,
    OfferStart = @OfferStart,
    OfferEnd = @OfferEnd
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.ItemId,
                entity.Price,
                entity.StockQuantity,
                entity.Sku,
                entity.ProductIdentifierType,
                entity.ProductIdentifierValue,
                entity.ImageUrls,
                entity.ThumbnailUrl,
                entity.ItemVariantName_en,
                entity.ItemVariantName_fr,
                entity.Deleted,
                entity.Offer,
                entity.OfferStart,
                entity.OfferEnd
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public async Task<bool> DeleteItemVariantAsync(Guid itemId, Guid variantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.ItemVariant 
SET Deleted = 1 
WHERE Id = @variantId AND ItemId = @itemId AND Deleted = 0";
            
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { variantId, itemId });
            
            if (rowsAffected > 0)
            {
                // Update the item's UpdatedAt timestamp
                var updateItemQuery = "UPDATE dbo.Item SET UpdatedAt = @UpdatedAt WHERE Id = @itemId";
                await dbConnection.ExecuteAsync(updateItemQuery, new { UpdatedAt = DateTime.UtcNow, itemId });
                return true;
            }
            
            return false;
        }
    }
}
