using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class TaxRateRepository(string connectionString) : ITaxRateRepository
    {
        protected IDbConnection dbConnection = new SqlConnection(connectionString);


        public async Task<TaxRate> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT TOP(1) * 
FROM dbo.TaxRate 
WHERE ID = @id";
            
            return await dbConnection.QueryFirstAsync<TaxRate>(query, new { id });
        }

        public async Task<IEnumerable<TaxRate>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.QueryAsync<TaxRate>("SELECT * FROM dbo.TaxRate");
        }

        public async Task<IEnumerable<TaxRate>> FindAsync(Func<TaxRate, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var taxRates = await dbConnection.QueryAsync<TaxRate>("SELECT * FROM dbo.TaxRate");
            return taxRates.Where(predicate);
        }

        public async Task<int> CountAsync(Func<TaxRate, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var taxRates = await dbConnection.QueryAsync<TaxRate>("SELECT * FROM dbo.TaxRate");
            return taxRates.Count(predicate);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.TaxRate WHERE ID = @id", new { id });
        }

        // ITaxRateRepository specific methods
        public async Task<IEnumerable<TaxRate>> FindByCountryAsync(string country)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT * 
FROM dbo.TaxRate 
WHERE Country = @country";
            
            return await dbConnection.QueryAsync<TaxRate>(query, new { country });
        }

        public async Task<IEnumerable<TaxRate>> FindByProvinceStateAsync(string country, string provinceState)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT * 
FROM dbo.TaxRate 
WHERE Country = @country AND ProvinceState = @provinceState";
            
            return await dbConnection.QueryAsync<TaxRate>(query, new { country, provinceState });
        }

        public async Task<IEnumerable<TaxRate>> FindActiveAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT * 
FROM dbo.TaxRate 
WHERE IsActive = 1";
            
            return await dbConnection.QueryAsync<TaxRate>(query);
        }

        public async Task<IEnumerable<TaxRate>> FindByActiveStatusAsync(bool isActive)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT * 
FROM dbo.TaxRate 
WHERE IsActive = @isActive";
            
            return await dbConnection.QueryAsync<TaxRate>(query, new { isActive });
        }

        public async Task<bool> ExistsByNameAndLocationAsync(string nameEn, string country, string? provinceState)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
SELECT COUNT(1) 
FROM dbo.TaxRate 
WHERE Name_en = @nameEn AND Country = @country AND 
      (ProvinceState = @provinceState OR (ProvinceState IS NULL AND @provinceState IS NULL))";
            
            return await dbConnection.ExecuteScalarAsync<bool>(query, new { nameEn, country, provinceState });
        }
    }
}