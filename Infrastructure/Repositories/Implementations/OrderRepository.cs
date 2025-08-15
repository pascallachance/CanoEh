using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class OrderRepository(string connectionString) : GenericRepository<Order>(connectionString), IOrderRepository
    {
        public override async Task<Order> AddAsync(Order entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.[Order] (
    ID,
    UserID,
    OrderDate,
    StatusID,
    Subtotal,
    TaxTotal,
    ShippingTotal,
    GrandTotal,
    Notes,
    CreatedAt,
    UpdatedAt)
OUTPUT INSERTED.OrderNumber
VALUES (
    @ID,
    @UserID,
    @OrderDate,
    @StatusID,
    @Subtotal,
    @TaxTotal,
    @ShippingTotal,
    @GrandTotal,
    @Notes,
    @CreatedAt,
    @UpdatedAt)";

            var parameters = new
            {
                entity.ID,
                entity.UserID,
                entity.OrderDate,
                entity.StatusID,
                entity.Subtotal,
                entity.TaxTotal,
                entity.ShippingTotal,
                entity.GrandTotal,
                entity.Notes,
                entity.CreatedAt,
                entity.UpdatedAt
            };

            var orderNumber = await dbConnection.QuerySingleAsync<int>(query, parameters);
            entity.OrderNumber = orderNumber;
            return entity;
        }

        public override async Task<Order> UpdateAsync(Order entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[Order] SET
    StatusID = @StatusID,
    Subtotal = @Subtotal,
    TaxTotal = @TaxTotal,
    ShippingTotal = @ShippingTotal,
    GrandTotal = @GrandTotal,
    Notes = @Notes,
    UpdatedAt = @UpdatedAt
WHERE ID = @ID";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task DeleteAsync(Order entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.[Order] WHERE ID = @ID";
            await dbConnection.ExecuteAsync(query, new { entity.ID });
        }

        public override async Task<Order> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.[Order] WHERE ID = @id";
            return await dbConnection.QuerySingleAsync<Order>(query, new { id });
        }

        public override async Task<IEnumerable<Order>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.[Order] ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<Order>(query);
        }

        public override async Task<IEnumerable<Order>> FindAsync(Func<Order, bool> predicate)
        {
            var allOrders = await GetAllAsync();
            return allOrders.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<Order, bool> predicate)
        {
            var allOrders = await GetAllAsync();
            return allOrders.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.[Order] WHERE ID = @id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { id });
            return count > 0;
        }

        public async Task<IEnumerable<Order>> FindByUserIdAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.[Order] WHERE UserID = @userId ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<Order>(query, new { userId });
        }

        public async Task<Order?> FindByUserIdAndIdAsync(Guid userId, Guid orderId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.[Order] WHERE UserID = @userId AND ID = @orderId";
            return await dbConnection.QuerySingleOrDefaultAsync<Order>(query, new { userId, orderId });
        }

        public async Task<Order?> FindByUserIdAndOrderNumberAsync(Guid userId, int orderNumber)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.[Order] WHERE UserID = @userId AND OrderNumber = @orderNumber";
            return await dbConnection.QuerySingleOrDefaultAsync<Order>(query, new { userId, orderNumber });
        }

        public async Task<bool> CanUserModifyOrderAsync(Guid userId, Guid orderId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT COUNT(1) 
FROM dbo.[Order] o
INNER JOIN dbo.OrderStatus os ON o.StatusID = os.ID
WHERE o.UserID = @userId 
  AND o.ID = @orderId 
  AND os.StatusCode IN ('Pending', 'Paid', 'Processing')";
            
            var count = await dbConnection.QuerySingleAsync<int>(query, new { userId, orderId });
            return count > 0;
        }

        public async Task<IEnumerable<Order>> FindByUserIdAndStatusAsync(Guid userId, string statusCode)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT o.* 
FROM dbo.[Order] o
INNER JOIN dbo.OrderStatus os ON o.StatusID = os.ID
WHERE o.UserID = @userId AND os.StatusCode = @statusCode
ORDER BY o.CreatedAt DESC";
            
            return await dbConnection.QueryAsync<Order>(query, new { userId, statusCode });
        }
    }
}