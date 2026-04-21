using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class ItemReviewRepository(string connectionString) : GenericRepository<ItemReview>(connectionString), IItemReviewRepository
    {
        public override async Task<ItemReview> AddAsync(ItemReview entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
INSERT INTO dbo.ItemReview (ItemID, UserID, Rating, ReviewText, CreatedAt, UpdatedAt)
OUTPUT INSERTED.Id
VALUES (@ItemID, @UserID, @Rating, @ReviewText, @CreatedAt, @UpdatedAt)";

            var id = await dbConnection.ExecuteScalarAsync<Guid>(query, entity);
            entity.Id = id;
            return entity;
        }

        public override async Task<ItemReview> UpdateAsync(ItemReview entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
UPDATE dbo.ItemReview
SET Rating = @Rating,
    ReviewText = @ReviewText,
    UpdatedAt = @UpdatedAt
WHERE Id = @Id";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task DeleteAsync(ItemReview entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            await dbConnection.ExecuteAsync("DELETE FROM dbo.ItemReview WHERE Id = @Id", new { entity.Id });
        }

        public override async Task<ItemReview> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var review = await dbConnection.QueryFirstOrDefaultAsync<ItemReview>(
                "SELECT * FROM dbo.ItemReview WHERE Id = @id",
                new { id });

            return review ?? throw new InvalidOperationException($"ItemReview with id {id} not found");
        }

        public override async Task<IEnumerable<ItemReview>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            return await dbConnection.QueryAsync<ItemReview>("SELECT * FROM dbo.ItemReview ORDER BY CreatedAt DESC");
        }

        public override async Task<IEnumerable<ItemReview>> FindAsync(Func<ItemReview, bool> predicate)
        {
            var reviews = await GetAllAsync();
            return reviews.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<ItemReview, bool> predicate)
        {
            var reviews = await GetAllAsync();
            return reviews.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            return await dbConnection.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM dbo.ItemReview WHERE Id = @id",
                new { id });
        }

        public async Task<ItemReview?> GetByUserAndItemAsync(Guid userId, Guid itemId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            const string query = "SELECT TOP(1) * FROM dbo.ItemReview WHERE UserID = @userId AND ItemID = @itemId";
            return await dbConnection.QueryFirstOrDefaultAsync<ItemReview>(query, new { userId, itemId });
        }

        public async Task<IEnumerable<ItemReview>> GetByItemIdAsync(Guid itemId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            const string query = "SELECT * FROM dbo.ItemReview WHERE ItemID = @itemId ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<ItemReview>(query, new { itemId });
        }

        public async Task<IEnumerable<ItemRatingSummary>> GetRatingSummariesAsync(IEnumerable<Guid> itemIds)
        {
            var ids = itemIds?.Distinct().ToList() ?? [];
            if (ids.Count == 0)
            {
                return [];
            }

            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT
    ItemID,
    CAST(AVG(CAST(Rating AS DECIMAL(10,2))) AS DECIMAL(10,2)) AS AverageRating,
    COUNT(1) AS RatingCount
FROM dbo.ItemReview
WHERE ItemID IN @ids
GROUP BY ItemID";

            return await dbConnection.QueryAsync<ItemRatingSummary>(query, new { ids });
        }

        public async Task<bool> HasUserPurchasedItemAsync(Guid userId, Guid itemId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT COUNT(1)
FROM dbo.[Order] o
INNER JOIN dbo.OrderItem oi ON o.ID = oi.OrderID
WHERE o.UserID = @userId
  AND oi.ItemID = @itemId";

            return await dbConnection.ExecuteScalarAsync<bool>(query, new { userId, itemId });
        }

        public async Task<IEnumerable<ReviewReminderCandidate>> GetPendingReviewReminderCandidatesAsync(DateTime cutoffUtc)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT
    o.UserID,
    u.Email,
    oi.ItemID,
    i.Name_en AS ItemName_en,
    i.Name_fr AS ItemName_fr,
    oi.DeliveredAt
FROM dbo.[Order] o
INNER JOIN dbo.OrderItem oi ON oi.OrderID = o.ID
INNER JOIN dbo.[User] u ON u.ID = o.UserID
INNER JOIN dbo.Item i ON i.Id = oi.ItemID
LEFT JOIN dbo.ItemReview ir ON ir.ItemID = oi.ItemID AND ir.UserID = o.UserID
WHERE oi.DeliveredAt IS NOT NULL
  AND oi.DeliveredAt <= @cutoffUtc
  AND ir.Id IS NULL";

            return await dbConnection.QueryAsync<ReviewReminderCandidate>(query, new { cutoffUtc });
        }
    }
}
