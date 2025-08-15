using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class OrderItemStatusRepository(string connectionString) : GenericRepository<OrderItemStatus>(connectionString), IOrderItemStatusRepository
    {
        public override async Task<OrderItemStatus> AddAsync(OrderItemStatus entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.OrderItemStatus (
    StatusCode,
    Name_en,
    Name_fr)
OUTPUT INSERTED.ID
VALUES (
    @StatusCode,
    @Name_en,
    @Name_fr)";

            var id = await dbConnection.QuerySingleAsync<int>(query, entity);
            entity.ID = id;
            return entity;
        }

        public override async Task<OrderItemStatus> UpdateAsync(OrderItemStatus entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderItemStatus SET
    StatusCode = @StatusCode,
    Name_en = @Name_en,
    Name_fr = @Name_fr
WHERE ID = @ID";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task DeleteAsync(OrderItemStatus entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.OrderItemStatus WHERE ID = @ID";
            await dbConnection.ExecuteAsync(query, new { entity.ID });
        }

        public override async Task<OrderItemStatus> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderItemStatus WHERE ID = @id";
            return await dbConnection.QuerySingleAsync<OrderItemStatus>(query, new { id });
        }

        public override async Task<IEnumerable<OrderItemStatus>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderItemStatus ORDER BY ID";
            return await dbConnection.QueryAsync<OrderItemStatus>(query);
        }

        public override async Task<IEnumerable<OrderItemStatus>> FindAsync(Func<OrderItemStatus, bool> predicate)
        {
            var allOrderItemStatuses = await GetAllAsync();
            return allOrderItemStatuses.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<OrderItemStatus, bool> predicate)
        {
            var allOrderItemStatuses = await GetAllAsync();
            return allOrderItemStatuses.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.OrderItemStatus WHERE ID = @id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { id });
            return count > 0;
        }

        public async Task<OrderItemStatus?> FindByStatusCodeAsync(string statusCode)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderItemStatus WHERE StatusCode = @statusCode";
            return await dbConnection.QuerySingleOrDefaultAsync<OrderItemStatus>(query, new { statusCode });
        }

        public async Task<IEnumerable<OrderItemStatus>> GetAllActiveAsync()
        {
            return await GetAllAsync();
        }
    }
}