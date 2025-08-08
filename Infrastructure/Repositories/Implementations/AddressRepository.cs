using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class AddressRepository(string connectionString) : GenericRepository<Address>(connectionString), IAddressRepository
    {
        public override async Task<Address> AddAsync(Address entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.Address (
    UserId,
    Street, 
    City, 
    State, 
    PostalCode, 
    Country, 
    AddressType,
    CreatedAt, 
    UpdatedAt)
OUTPUT INSERTED.Id
VALUES (
    @UserId,
    @Street, 
    @City, 
    @State, 
    @PostalCode, 
    @Country, 
    @AddressType,
    @CreatedAt, 
    @UpdatedAt)";

            var parameters = new
            {
                entity.UserId,
                entity.Street,
                entity.City,
                entity.State,
                entity.PostalCode,
                entity.Country,
                entity.AddressType,
                entity.CreatedAt,
                entity.UpdatedAt
            };

            var id = await dbConnection.QuerySingleAsync<Guid>(query, parameters);
            entity.Id = id;
            return entity;
        }

        public override async Task<Address> UpdateAsync(Address entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.Address 
SET 
    Street = @Street,
    City = @City,
    State = @State,
    PostalCode = @PostalCode,
    Country = @Country,
    AddressType = @AddressType,
    UpdatedAt = @UpdatedAt
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.Street,
                entity.City,
                entity.State,
                entity.PostalCode,
                entity.Country,
                entity.AddressType,
                entity.UpdatedAt
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override async Task DeleteAsync(Address entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.Address WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<Address> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.Address WHERE Id = @Id";
            return await dbConnection.QuerySingleOrDefaultAsync<Address>(query, new { Id = id });
        }

        public override async Task<IEnumerable<Address>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.Address";
            return await dbConnection.QueryAsync<Address>(query);
        }

        public override async Task<IEnumerable<Address>> FindAsync(Func<Address, bool> predicate)
        {
            var addresses = await GetAllAsync();
            return addresses.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<Address, bool> predicate)
        {
            var addresses = await GetAllAsync();
            return addresses.Where(predicate).Count();
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.Address WHERE Id = @Id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.Address WHERE UserId = @UserId";
            return await dbConnection.QueryAsync<Address>(query, new { UserId = userId });
        }

        public async Task<IEnumerable<Address>> GetByUserIdAndTypeAsync(Guid userId, string addressType)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.Address WHERE UserId = @UserId AND AddressType = @AddressType";
            return await dbConnection.QueryAsync<Address>(query, new { UserId = userId, AddressType = addressType });
        }

        public async Task<bool> ExistsByUserIdAsync(Guid userId, Guid addressId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.Address WHERE Id = @AddressId AND UserId = @UserId";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { AddressId = addressId, UserId = userId });
            return count > 0;
        }
    }
}