using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class ItemRepository(string connectionString) : GenericRepository<Item>(connectionString), IItemRepository
    {
        private const string AllActiveItemsQuery = "SELECT * FROM dbo.Item WHERE Deleted = 0";

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
    CategoryNodeID, 
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
    @CategoryNodeID, 
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
                entity.CategoryNodeID,
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

            // Items-only query; does not load variants. Use GetAllWithVariantsAsync() when variants are needed.
            return await dbConnection.QueryAsync<Item>(AllActiveItemsQuery);
        }

        public async Task<IEnumerable<Item>> GetAllWithVariantsAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var items = (await dbConnection.QueryAsync<Item>(AllActiveItemsQuery)).ToList();

            if (!items.Any())
            {
                return items;
            }

            var itemIds = items.Select(i => i.Id).ToList();

            // Get all ItemVariants for the items (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);

            return items;
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
    CategoryNodeID = @CategoryNodeID,
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
                entity.CategoryNodeID,
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

            var query = "SELECT * FROM dbo.Item WHERE Id = @id AND Deleted = 0";
            var item = await dbConnection.QueryFirstOrDefaultAsync<Item>(query, new { id });

            if (item == null)
            {
                return null;
            }

            // Load variants for the item (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId = @id AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { id })).ToList();

            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(new List<Item> { item }, variants);

            return item;
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
            
            // Get all ItemVariants for the items (optionally include deleted variants)
            var variantQuery = includeDeleted
                ? "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds"
                : "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());
            
            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);
            
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
            
            // Get all ItemVariants for the items (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());
            
            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);
            
            return items;
        }

        public async Task<IEnumerable<Item>> GetSuggestedProductsAsync(int count = 4)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            // Get random items that have at least one variant with ImageUrls or ThumbnailUrl
            // Use NEWID() for random selection and ensure we only get one variant per item
            var itemQuery = @"
                WITH ItemsWithImages AS (
                    SELECT DISTINCT i.Id
                    FROM dbo.Item i
                    INNER JOIN dbo.ItemVariant iv ON i.Id = iv.ItemId
                    WHERE i.Deleted = 0 
                      AND iv.Deleted = 0
                      AND (
                          (iv.ImageUrls IS NOT NULL AND iv.ImageUrls != '')
                          OR (iv.ThumbnailUrl IS NOT NULL AND iv.ThumbnailUrl != '')
                      )
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
            
            // Get all ItemVariants for the items (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());
            
            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);
            
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
            
            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);
            
            return items;
        }

        public async Task<IEnumerable<Item>> GetSuggestedCategoriesProductsAsync(int count = 4)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Get one item per category (up to @count different categories) randomly selected
            var itemQuery = @"
                WITH RankedItems AS (
                    SELECT i.Id,
                           ROW_NUMBER() OVER (PARTITION BY i.CategoryNodeID ORDER BY NEWID()) AS rn
                    FROM dbo.Item i
                    WHERE i.Deleted = 0
                )
                SELECT TOP (@count) i.*
                FROM dbo.Item i
                INNER JOIN RankedItems r ON i.Id = r.Id AND r.rn = 1
                ORDER BY NEWID()";
            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { count })).ToList();

            if (!items.Any())
            {
                return items;
            }

            var itemIds = items.Select(i => i.Id).ToList();

            // Get all ItemVariants for the items (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);

            return items;
        }

        public async Task<IEnumerable<Item>> GetBestRatedProductsAsync(int count = 100)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Get the top N non-deleted items ordered by average rating descending, then by rating count descending
            var itemQuery = @"
                WITH RatingSummary AS (
                    SELECT ItemID,
                           CAST(AVG(Rating) AS DECIMAL(10,2)) AS AverageRating,
                           COUNT(1) AS RatingCount
                    FROM dbo.ItemReview
                    GROUP BY ItemID
                )
                SELECT TOP (@count) i.*
                FROM dbo.Item i
                INNER JOIN RatingSummary rs ON i.Id = rs.ItemID
                WHERE i.Deleted = 0
                ORDER BY rs.AverageRating DESC, rs.RatingCount DESC";
            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { count })).ToList();

            if (!items.Any())
            {
                return items;
            }

            var itemIds = items.Select(i => i.Id).ToList();

            // Get all ItemVariants for the items (exclude deleted variants)
            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);

            return items;
        }

        public async Task<IEnumerable<Item>> GetItemsByCategoryNodeAsync(Guid nodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Use a recursive CTE to find the given node and all its descendants,
            // then return all non-deleted items assigned to any of those nodes.
            // AncestorPath tracks visited node IDs (comma-separated) to break cycles
            // caused by corrupted ParentId chains in the database.
            var itemQuery = @"
                WITH DescendantNodes AS (
                    SELECT Id, CAST(Id AS NVARCHAR(MAX)) AS AncestorPath
                    FROM dbo.CategoryNode WHERE Id = @nodeId
                    UNION ALL
                    SELECT c.Id, dn.AncestorPath + ',' + CAST(c.Id AS NVARCHAR(36))
                    FROM dbo.CategoryNode c
                    INNER JOIN DescendantNodes dn ON c.ParentId = dn.Id
                    WHERE ',' + dn.AncestorPath + ',' NOT LIKE '%,' + CAST(c.Id AS NVARCHAR(36)) + ',%'
                )
                SELECT i.*
                FROM dbo.Item i
                WHERE i.CategoryNodeID IN (SELECT Id FROM DescendantNodes)
                  AND i.Deleted = 0
                ORDER BY i.CreatedAt DESC
                OPTION (MAXRECURSION 32767)";

            var items = (await dbConnection.QueryAsync<Item>(itemQuery, new { nodeId })).ToList();

            if (!items.Any())
            {
                return items;
            }

            var itemIds = items.Select(i => i.Id).ToList();

            var variantQuery = "SELECT * FROM dbo.ItemVariant WHERE ItemId IN @itemIds AND Deleted = 0";
            var variants = (await dbConnection.QueryAsync<ItemVariant>(variantQuery, new { itemIds })).ToList();
            var variantsByItemId = variants.GroupBy(v => v.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            await HydrateVariantAttributesAndFeaturesAsync(variants);
            AssignVariantsToItems(items, variantsByItemId);

            return items;
        }

        /// <summary>
        /// Loads and attaches <see cref="ItemVariantAttribute"/> and <see cref="ItemVariantFeatures"/> records
        /// to the supplied variants in a single pair of batch queries. No-ops when the list is empty.
        /// </summary>
        private async Task HydrateVariantAttributesAndFeaturesAsync(List<ItemVariant> variants)
        {
            if (variants.Count == 0)
            {
                return;
            }

            var variantIds = variants.Select(v => v.Id).ToList();

            // Load variant attributes in one batch query and group by variant ID for O(1) lookup
            var variantAttributeQuery = "SELECT * FROM dbo.ItemVariantAttribute WHERE ItemVariantID IN @variantIds";
            var variantAttributes = (await dbConnection.QueryAsync<ItemVariantAttribute>(variantAttributeQuery, new { variantIds })).ToList();
            var variantAttributesByVariantId = variantAttributes.GroupBy(va => va.ItemVariantID).ToDictionary(g => g.Key, g => g.ToList());

            // Load variant features in one batch query and group by variant ID for O(1) lookup
            var variantFeaturesQuery = "SELECT * FROM dbo.ItemVariantFeatures WHERE ItemVariantID IN @variantIds";
            var variantFeatures = (await dbConnection.QueryAsync<ItemVariantFeatures>(variantFeaturesQuery, new { variantIds })).ToList();
            var variantFeaturesByVariantId = variantFeatures.GroupBy(vf => vf.ItemVariantID).ToDictionary(g => g.Key, g => g.ToList());

            // Assign attributes and features to their respective variants
            foreach (var variant in variants)
            {
                variant.ItemVariantAttributes = variantAttributesByVariantId.TryGetValue(variant.Id, out var attrs) ? attrs : new List<ItemVariantAttribute>();
                variant.ItemVariantFeatures = variantFeaturesByVariantId.TryGetValue(variant.Id, out var features) ? features : new List<ItemVariantFeatures>();
            }
        }

        /// <summary>
        /// Assigns the pre-loaded (and already hydrated) variants to their parent items and
        /// aggregates item-level features from the assigned variants.
        /// </summary>
        private static void AssignVariantsToItems(List<Item> items, Dictionary<Guid, List<ItemVariant>> variantsByItemId)
        {
            foreach (var item in items)
            {
                item.Variants = variantsByItemId.TryGetValue(item.Id, out var vars) ? vars : new List<ItemVariant>();
                item.ItemVariantFeatures = (item.Variants ?? Enumerable.Empty<ItemVariant>())
                    .SelectMany(v => v.ItemVariantFeatures ?? Enumerable.Empty<ItemVariantFeatures>())
                    .ToList();
            }
        }

        /// <summary>
        /// Assigns the pre-loaded (and already hydrated) variants directly to a single item and
        /// aggregates item-level features from the assigned variants.
        /// </summary>
        private static void AssignVariantsToItems(List<Item> items, List<ItemVariant> variants)
        {
            var item = items[0];
            item.Variants = variants;
            item.ItemVariantFeatures = variants
                .SelectMany(v => v.ItemVariantFeatures ?? Enumerable.Empty<ItemVariantFeatures>())
                .ToList();
        }
    }
}