using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class OrderItemRepository(string connectionString) : GenericRepository<OrderItem>(connectionString), IOrderItemRepository
    {
        public override async Task<OrderItem> AddAsync(OrderItem entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.OrderItem (
    ID,
    OrderID,
    ItemID,
    ItemVariantID,
    Name_en,
    Name_fr,
    VariantName_en,
    VariantName_fr,
    Quantity,
    UnitPrice,
    TotalPrice,
    Status,
    DeliveredAt,
    OnHoldReason)
VALUES (
    @ID,
    @OrderID,
    @ItemID,
    @ItemVariantID,
    @Name_en,
    @Name_fr,
    @VariantName_en,
    @VariantName_fr,
    @Quantity,
    @UnitPrice,
    @TotalPrice,
    @Status,
    @DeliveredAt,
    @OnHoldReason)";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task<OrderItem> UpdateAsync(OrderItem entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderItem SET
    Quantity = @Quantity,
    UnitPrice = @UnitPrice,
    TotalPrice = @TotalPrice,
    Status = @Status,
    DeliveredAt = @DeliveredAt,
    OnHoldReason = @OnHoldReason
WHERE ID = @ID";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task DeleteAsync(OrderItem entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.OrderItem WHERE ID = @ID";
            await dbConnection.ExecuteAsync(query, new { entity.ID });
        }

        public override async Task<OrderItem> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderItem WHERE ID = @id";
            return await dbConnection.QuerySingleAsync<OrderItem>(query, new { id });
        }

        public override async Task<IEnumerable<OrderItem>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderItem";
            return await dbConnection.QueryAsync<OrderItem>(query);
        }

        public override async Task<IEnumerable<OrderItem>> FindAsync(Func<OrderItem, bool> predicate)
        {
            var allOrderItems = await GetAllAsync();
            return allOrderItems.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<OrderItem, bool> predicate)
        {
            var allOrderItems = await GetAllAsync();
            return allOrderItems.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.OrderItem WHERE ID = @id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { id });
            return count > 0;
        }

        public async Task<IEnumerable<OrderItem>> FindByOrderIdAsync(Guid orderId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderItem WHERE OrderID = @orderId";
            return await dbConnection.QueryAsync<OrderItem>(query, new { orderId });
        }

        public async Task<IEnumerable<OrderItem>> FindByOrderIdAndStatusAsync(Guid orderId, string status)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderItem WHERE OrderID = @orderId AND Status = @status";
            return await dbConnection.QueryAsync<OrderItem>(query, new { orderId, status });
        }

        public async Task<bool> UpdateStatusAsync(Guid orderItemId, string status, DateTime? deliveredAt = null, string? onHoldReason = null)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderItem SET
    Status = @status,
    DeliveredAt = @deliveredAt,
    OnHoldReason = @onHoldReason
WHERE ID = @orderItemId";

            var rowsAffected = await dbConnection.ExecuteAsync(query, new { orderItemId, status, deliveredAt, onHoldReason });
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateStatusBulkAsync(IEnumerable<Guid> orderItemIds, string status, DateTime? deliveredAt = null, string? onHoldReason = null)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderItem SET
    Status = @status,
    DeliveredAt = @deliveredAt,
    OnHoldReason = @onHoldReason
WHERE ID IN @orderItemIds";

            var rowsAffected = await dbConnection.ExecuteAsync(query, new { orderItemIds, status, deliveredAt, onHoldReason });
            return rowsAffected > 0;
        }
    }
}