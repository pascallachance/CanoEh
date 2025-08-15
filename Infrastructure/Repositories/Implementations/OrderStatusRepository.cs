using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class OrderStatusRepository(string connectionString) : GenericRepository<OrderStatus>(connectionString), IOrderStatusRepository
    {
        public override async Task<OrderStatus> AddAsync(OrderStatus entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.OrderStatus (
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

        public override async Task<OrderStatus> UpdateAsync(OrderStatus entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderStatus SET
    StatusCode = @StatusCode,
    Name_en = @Name_en,
    Name_fr = @Name_fr
WHERE ID = @ID";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task DeleteAsync(OrderStatus entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.OrderStatus WHERE ID = @ID";
            await dbConnection.ExecuteAsync(query, new { entity.ID });
        }

        public override async Task<OrderStatus> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderStatus WHERE ID = @id";
            return await dbConnection.QuerySingleAsync<OrderStatus>(query, new { id });
        }

        public override async Task<IEnumerable<OrderStatus>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderStatus ORDER BY ID";
            return await dbConnection.QueryAsync<OrderStatus>(query);
        }

        public override async Task<IEnumerable<OrderStatus>> FindAsync(Func<OrderStatus, bool> predicate)
        {
            var allOrderStatuses = await GetAllAsync();
            return allOrderStatuses.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<OrderStatus, bool> predicate)
        {
            var allOrderStatuses = await GetAllAsync();
            return allOrderStatuses.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.OrderStatus WHERE ID = @id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { id });
            return count > 0;
        }

        public async Task<OrderStatus?> FindByStatusCodeAsync(string statusCode)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderStatus WHERE StatusCode = @statusCode";
            return await dbConnection.QuerySingleOrDefaultAsync<OrderStatus>(query, new { statusCode });
        }

        public async Task<IEnumerable<OrderStatus>> GetAllActiveAsync()
        {
            return await GetAllAsync();
        }
    }
}