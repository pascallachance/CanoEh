using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class OrderAddressRepository(string connectionString) : GenericRepository<OrderAddress>(connectionString), IOrderAddressRepository
    {
        public override async Task<OrderAddress> AddAsync(OrderAddress entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.OrderAddress (
    ID,
    OrderID,
    Type,
    FullName,
    AddressLine1,
    AddressLine2,
    AddressLine3,
    City,
    ProvinceState,
    PostalCode,
    Country)
VALUES (
    @ID,
    @OrderID,
    @Type,
    @FullName,
    @AddressLine1,
    @AddressLine2,
    @AddressLine3,
    @City,
    @ProvinceState,
    @PostalCode,
    @Country)";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task<OrderAddress> UpdateAsync(OrderAddress entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderAddress SET
    Type = @Type,
    FullName = @FullName,
    AddressLine1 = @AddressLine1,
    AddressLine2 = @AddressLine2,
    AddressLine3 = @AddressLine3,
    City = @City,
    ProvinceState = @ProvinceState,
    PostalCode = @PostalCode,
    Country = @Country
WHERE ID = @ID";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task DeleteAsync(OrderAddress entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.OrderAddress WHERE ID = @ID";
            await dbConnection.ExecuteAsync(query, new { entity.ID });
        }

        public override async Task<OrderAddress> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderAddress WHERE ID = @id";
            return await dbConnection.QuerySingleAsync<OrderAddress>(query, new { id });
        }

        public override async Task<IEnumerable<OrderAddress>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderAddress";
            return await dbConnection.QueryAsync<OrderAddress>(query);
        }

        public override async Task<IEnumerable<OrderAddress>> FindAsync(Func<OrderAddress, bool> predicate)
        {
            var allOrderAddresses = await GetAllAsync();
            return allOrderAddresses.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<OrderAddress, bool> predicate)
        {
            var allOrderAddresses = await GetAllAsync();
            return allOrderAddresses.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.OrderAddress WHERE ID = @id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { id });
            return count > 0;
        }

        public async Task<IEnumerable<OrderAddress>> FindByOrderIdAsync(Guid orderId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderAddress WHERE OrderID = @orderId";
            return await dbConnection.QueryAsync<OrderAddress>(query, new { orderId });
        }

        public async Task<OrderAddress?> FindByOrderIdAndTypeAsync(Guid orderId, string type)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderAddress WHERE OrderID = @orderId AND Type = @type";
            return await dbConnection.QuerySingleOrDefaultAsync<OrderAddress>(query, new { orderId, type });
        }
    }
}