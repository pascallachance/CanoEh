using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class CategoryMandatoryExtraAttributeRepository(string connectionString) : GenericRepository<CategoryMandatoryExtraAttribute>(connectionString), ICategoryMandatoryExtraAttributeRepository
    {
        public override async Task<CategoryMandatoryExtraAttribute> AddAsync(CategoryMandatoryExtraAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            var query = @"
INSERT INTO dbo.CategoryMandatoryExtraAttribute (Id, CategoryNodeId, Name_en, Name_fr, AttributeType, SortOrder)
VALUES (@Id, @CategoryNodeId, @Name_en, @Name_fr, @AttributeType, @SortOrder)";

            var parameters = new
            {
                entity.Id,
                entity.CategoryNodeId,
                entity.Name_en,
                entity.Name_fr,
                entity.AttributeType,
                entity.SortOrder
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override async Task<int> CountAsync(Func<CategoryMandatoryExtraAttribute, bool> predicate)
        {
            var attributes = await GetAllAsync();
            return attributes.Count(predicate);
        }

        public override async Task DeleteAsync(CategoryMandatoryExtraAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.CategoryMandatoryExtraAttribute WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.CategoryMandatoryExtraAttribute WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<CategoryMandatoryExtraAttribute>> FindAsync(Func<CategoryMandatoryExtraAttribute, bool> predicate)
        {
            var attributes = await GetAllAsync();
            return attributes.Where(predicate);
        }

        public override async Task<IEnumerable<CategoryMandatoryExtraAttribute>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryExtraAttribute ORDER BY CASE WHEN SortOrder IS NULL THEN 1 ELSE 0 END, SortOrder, Name_en";
            return await dbConnection.QueryAsync<CategoryMandatoryExtraAttribute>(query);
        }

        public override async Task<CategoryMandatoryExtraAttribute> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryExtraAttribute WHERE Id = @id";
            var result = await dbConnection.QueryFirstOrDefaultAsync<CategoryMandatoryExtraAttribute>(query, new { id });
            
            return result ?? throw new InvalidOperationException($"CategoryMandatoryExtraAttribute with id {id} not found");
        }

        public override async Task<CategoryMandatoryExtraAttribute> UpdateAsync(CategoryMandatoryExtraAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.CategoryMandatoryExtraAttribute 
SET CategoryNodeId = @CategoryNodeId, 
    Name_en = @Name_en, 
    Name_fr = @Name_fr, 
    AttributeType = @AttributeType, 
    SortOrder = @SortOrder
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.CategoryNodeId,
                entity.Name_en,
                entity.Name_fr,
                entity.AttributeType,
                entity.SortOrder
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public async Task<IEnumerable<CategoryMandatoryExtraAttribute>> GetAttributesByCategoryNodeIdAsync(Guid categoryNodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryExtraAttribute WHERE CategoryNodeId = @categoryNodeId ORDER BY CASE WHEN SortOrder IS NULL THEN 1 ELSE 0 END, SortOrder, Name_en";
            return await dbConnection.QueryAsync<CategoryMandatoryExtraAttribute>(query, new { categoryNodeId });
        }

        public async Task<bool> DeleteAttributesByCategoryNodeIdAsync(Guid categoryNodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.CategoryMandatoryExtraAttribute WHERE CategoryNodeId = @categoryNodeId";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { categoryNodeId });
            
            return rowsAffected > 0;
        }
    }
}
