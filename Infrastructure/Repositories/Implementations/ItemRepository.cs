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

        public async Task<IEnumerable<Item>> GetBySellerIdAsync(Guid sellerId, bool includeDeleted = false)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Get all items for the seller (optionally include deleted items)
            var itemQuery = includeDeleted 
                ? "SELECT * FROM dbo.Item WHERE SellerID = @sellerId"
                : "SELECT * FROM dbo.Item WHERE SellerID = @sellerId AND Deleted = 0";
            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { sellerId })).ToList();
            
            if (!items.Any())
            {
                return items;
            }
            
            var itemIds = items.Select(i => i.Id).ToList();
            
            // Get all ItemVariantFeatures for the items and group by ItemID for O(1) lookup
            var itemVariantFeaturesQuery = "SELECT * FROM dbo.ItemAttribute WHERE ItemID IN @itemIds";
            var itemVariantFeatures = (await dbConnection.QueryAsync<ItemVariantFeatures>(itemVariantFeaturesQuery, new { itemIds })).ToList();
            var itemVariantFeaturesByItemId = itemVariantFeatures.GroupBy(ia => ia.ItemID).ToDictionary(g => g.Key, g => g.ToList());
            
            // Get all ItemVariants for the items (optionally include deleted variants)
            var variantQuery = includeDeleted
                ? "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds"
                : "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
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
            
            // Assign ItemVariants and ItemVariantFeatures to their respective Items using dictionary lookup
            foreach (var item in items)
            {
                item.Variants = variantsByItemId.TryGetValue(item.Id, out var vars) ? vars : new List<ItemVariant>();
                item.ItemVariantFeatures = itemVariantFeaturesByItemId.TryGetValue(item.Id, out var attrs) ? attrs : new List<ItemVariantFeatures>();
            }
            
            return items;
        }

        public async Task<IEnumerable<Item>> GetRecentlyAddedProductsAsync(int count = 100)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Get the most recently added items (non-deleted) ordered by CreatedAt DESC
            var itemQuery = @"
                SELECT TOP (@count) * 
                FROM dbo.Item 
                WHERE Deleted = 0 
                ORDER BY CreatedAt DESC";
            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { count })).ToList();
            
            if (!items.Any())
            {
                return items;
            }
            
            var itemIds = items.Select(i => i.Id).ToList();
            
            // Get all ItemVariantFeatures for the items and group by ItemID for O(1) lookup
            var itemVariantFeaturesQuery = "SELECT * FROM dbo.ItemAttribute WHERE ItemID IN @itemIds";
            var itemVariantFeatures = (await dbConnection.QueryAsync<ItemVariantFeatures>(itemVariantFeaturesQuery, new { itemIds })).ToList();
            var itemVariantFeaturesByItemId = itemVariantFeatures.GroupBy(ia => ia.ItemID).ToDictionary(g => g.Key, g => g.ToList());
            
            // Get all ItemVariants for the items (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
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
            
            // Assign ItemVariants and ItemVariantFeatures to their respective Items using dictionary lookup
            foreach (var item in items)
            {
                item.Variants = variantsByItemId.TryGetValue(item.Id, out var vars) ? vars : new List<ItemVariant>();
                item.ItemVariantFeatures = itemVariantFeaturesByItemId.TryGetValue(item.Id, out var attrs) ? attrs : new List<ItemVariantFeatures>();
            }
            
            return items;
        }

        public async Task<IEnumerable<Item>> GetSuggestedProductsAsync(int count = 4)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Get random items that have at least one variant with ImageUrls
            // Use NEWID() for random selection and ensure we only get one variant per item
            var itemQuery = @"
                WITH ItemsWithImages AS (
                    SELECT DISTINCT i.Id
                    FROM dbo.Item i
                    INNER JOIN dbo.ItemVariant iv ON i.Id = iv.ItemId
                    WHERE i.Deleted = 0 
                      AND iv.Deleted = 0
                      AND (iv.ImageUrls IS NOT NULL AND iv.ImageUrls != '')
                )
                SELECT TOP (@count) * 
                FROM dbo.Item 
                WHERE Id IN (SELECT Id FROM ItemsWithImages)
                ORDER BY NEWID()";
            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { count })).ToList();
            
            if (!items.Any())
            {
                return items;
            }
            
            var itemIds = items.Select(i => i.Id).ToList();
            
            // Get all ItemVariantFeatures for the items and group by ItemID for O(1) lookup
            var itemVariantFeaturesQuery = "SELECT * FROM dbo.ItemAttribute WHERE ItemID IN @itemIds";
            var itemVariantFeatures = (await dbConnection.QueryAsync<ItemVariantFeatures>(itemVariantFeaturesQuery, new { itemIds })).ToList();
            var itemVariantFeaturesByItemId = itemVariantFeatures.GroupBy(ia => ia.ItemID).ToDictionary(g => g.Key, g => g.ToList());
            
            // Get all ItemVariants for the items (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
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
            
            // Assign ItemVariants and ItemVariantFeatures to their respective Items using dictionary lookup
            foreach (var item in items)
            {
                item.Variants = variantsByItemId.TryGetValue(item.Id, out var vars) ? vars : new List<ItemVariant>();
                item.ItemVariantFeatures = itemVariantFeaturesByItemId.TryGetValue(item.Id, out var attrs) ? attrs : new List<ItemVariantFeatures>();
            }
            
            return items;
        }

        public async Task<IEnumerable<Item>> GetProductsWithOffersAsync(int count = 10)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Get items that have at least one variant with an offer
            // Sort by best offer (highest percentage) first
            // Only include variants where offer is not null and current date fit between OfferStart and OfferEnd to keep only currently valid offers
            var itemQuery = @"
                WITH ItemsWithOffers AS (
                    SELECT DISTINCT i.Id, MAX(iv.Offer) AS BestOffer
                    FROM dbo.Item i
                    INNER JOIN dbo.ItemVariant iv ON i.Id = iv.ItemId
                    WHERE i.Deleted = 0 
                      AND iv.Deleted = 0
                      AND iv.Offer IS NOT NULL
                      AND iv.Offer > 0
                      AND (iv.OfferStart IS NULL OR iv.OfferStart <= GETUTCDATE())
                      AND (iv.OfferEnd IS NULL OR iv.OfferEnd >= GETUTCDATE())
                    GROUP BY i.Id
                )
                SELECT TOP (@count) i.* 
                FROM dbo.Item i
                INNER JOIN ItemsWithOffers iwo ON i.Id = iwo.Id
                ORDER BY iwo.BestOffer DESC, i.CreatedAt DESC";
            
            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { count })).ToList();
            
            if (!items.Any())
            {
                return items;
            }
            
            var itemIds = items.Select(i => i.Id).ToList();
            
            // Get all ItemVariantFeatures for the items and group by ItemID for O(1) lookup
            var itemVariantFeaturesQuery = "SELECT * FROM dbo.ItemAttribute WHERE ItemID IN @itemIds";
            var itemVariantFeatures = (await dbConnection.QueryAsync<ItemVariantFeatures>(itemVariantFeaturesQuery, new { itemIds })).ToList();
            var itemVariantFeaturesByItemId = itemVariantFeatures.GroupBy(ia => ia.ItemID).ToDictionary(g => g.Key, g => g.ToList());
            
            // Get ItemVariants with offers (exclude deleted variants)
            // Only include variants that have an offer within the date range
            var variantQuery = @"
                SELECT * FROM dbo.ItemVariant 
                WHERE ItemId IN @itemIds 
                  AND Deleted = 0 
                  AND Offer IS NOT NULL 
                  AND Offer > 0
                  AND (OfferStart IS NULL OR OfferStart <= GETUTCDATE())
                  AND (OfferEnd IS NULL OR OfferEnd >= GETUTCDATE())";
            
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
            
            // Assign ItemVariants and ItemVariantFeatures to their respective Items using dictionary lookup
            foreach (var item in items)
            {
                item.Variants = variantsByItemId.TryGetValue(item.Id, out var vars) ? vars : new List<ItemVariant>();
                item.ItemVariantFeatures = itemVariantFeaturesByItemId.TryGetValue(item.Id, out var attrs) ? attrs : new List<ItemVariantFeatures>();
            }
            
            return items;
        }
    }
}