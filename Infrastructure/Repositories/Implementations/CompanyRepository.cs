using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class CompanyRepository(string connectionString) : GenericRepository<Company>(connectionString), ICompanyRepository
    {
        public override async Task<Company> AddAsync(Company entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.Company (
    OwnerID,
    Name,
    Description,
    Logo,
    CreatedAt,
    UpdatedAt,
    CountryOfCitizenship,
    FullBirthName,
    CountryOfBirth,
    BirthDate,
    IdentityDocumentType,
    IdentityDocument,
    BankDocument,
    FacturationDocument,
    CompanyPhone,
    CompanyType,
    Address1,
    Address2,
    Address3,
    City,
    ProvinceState,
    Country,
    PostalCode)
OUTPUT INSERTED.Id
VALUES (
    @OwnerID,
    @Name,
    @Description,
    @Logo,
    @CreatedAt,
    @UpdatedAt,
    @CountryOfCitizenship,
    @FullBirthName,
    @CountryOfBirth,
    @BirthDate,
    @IdentityDocumentType,
    @IdentityDocument,
    @BankDocument,
    @FacturationDocument,
    @CompanyPhone,
    @CompanyType,
    @Address1,
    @Address2,
    @Address3,
    @City,
    @ProvinceState,
    @Country,
    @PostalCode)";

            var parameters = new
            {
                entity.OwnerID,
                entity.Name,
                entity.Description,
                entity.Logo,
                entity.CreatedAt,
                entity.UpdatedAt,
                entity.CountryOfCitizenship,
                entity.FullBirthName,
                entity.CountryOfBirth,
                entity.BirthDate,
                entity.IdentityDocumentType,
                entity.IdentityDocument,
                entity.BankDocument,
                entity.FacturationDocument,
                entity.CompanyPhone,
                entity.CompanyType,
                entity.Address1,
                entity.Address2,
                entity.Address3,
                entity.City,
                entity.ProvinceState,
                entity.Country,
                entity.PostalCode
            };
            Guid newCompanyId = await dbConnection.ExecuteScalarAsync<Guid>(query, parameters);
            entity.Id = newCompanyId; 
            return entity;
        }

        public override async Task<int> CountAsync(Func<Company, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var companies = await dbConnection.QueryAsync<Company>("SELECT * FROM dbo.Company");
            return companies.Count(predicate);
        }

        public override async Task DeleteAsync(Company entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.Company WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Company WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<Company>> FindAsync(Func<Company, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var companies = await dbConnection.QueryAsync<Company>("SELECT * FROM dbo.Company");
            return companies.Where(predicate);
        }

        public override async Task<IEnumerable<Company>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.QueryAsync<Company>("SELECT * FROM dbo.Company");
        }

        public override async Task<Company> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Company 
WHERE Id = @id";
            return await dbConnection.QueryFirstAsync<Company>(query, new { id });
        }

        public override async Task<Company> UpdateAsync(Company entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.Company
SET
    OwnerID = @OwnerID,
    Name = @Name,
    Description = @Description,
    Logo = @Logo,
    CreatedAt = @CreatedAt,
    UpdatedAt = @UpdatedAt,
    CountryOfCitizenship = @CountryOfCitizenship,
    FullBirthName = @FullBirthName,
    CountryOfBirth = @CountryOfBirth,
    BirthDate = @BirthDate,
    IdentityDocumentType = @IdentityDocumentType,
    IdentityDocument = @IdentityDocument,
    BankDocument = @BankDocument,
    FacturationDocument = @FacturationDocument,
    CompanyPhone = @CompanyPhone,
    CompanyType = @CompanyType,
    Address1 = @Address1,
    Address2 = @Address2,
    Address3 = @Address3,
    City = @City,
    ProvinceState = @ProvinceState,
    Country = @Country,
    PostalCode = @PostalCode
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.OwnerID,
                entity.Name,
                entity.Description,
                entity.Logo,
                entity.CreatedAt,
                entity.UpdatedAt,
                entity.CountryOfCitizenship,
                entity.FullBirthName,
                entity.CountryOfBirth,
                entity.BirthDate,
                entity.IdentityDocumentType,
                entity.IdentityDocument,
                entity.BankDocument,
                entity.FacturationDocument,
                entity.CompanyPhone,
                entity.CompanyType,
                entity.Address1,
                entity.Address2,
                entity.Address3,
                entity.City,
                entity.ProvinceState,
                entity.Country,
                entity.PostalCode
            };
            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        // ICompanyRepository specific methods
        public async Task<IEnumerable<Company>> FindByOwnerAsync(Guid ownerId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT * 
FROM dbo.Company 
WHERE OwnerID = @ownerId";
            return await dbConnection.QueryAsync<Company>(query, new { ownerId });
        }

        public async Task<Company?> FindByNameAsync(string name)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Company 
WHERE Name = @name";
            return await dbConnection.QueryFirstOrDefaultAsync<Company>(query, new { name });
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Company WHERE Name = @name", new { name });
        }

        public async Task<bool> IsOwnerAsync(Guid companyId, Guid ownerId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Company WHERE Id = @companyId AND OwnerID = @ownerId", new { companyId, ownerId });
        }
    }
}