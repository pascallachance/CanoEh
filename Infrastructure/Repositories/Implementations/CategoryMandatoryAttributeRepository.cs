using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class CategoryMandatoryAttributeRepository(string connectionString) : GenericRepository<CategoryMandatoryAttribute>(connectionString), ICategoryMandatoryAttributeRepository
    {
        public override async Task<CategoryMandatoryAttribute> AddAsync(CategoryMandatoryAttribute entity)
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
INSERT INTO dbo.CategoryMandatoryAttribute (Id, CategoryNodeId, Name_en, Name_fr, AttributeType, SortOrder)
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

        public override async Task<int> CountAsync(Func<CategoryMandatoryAttribute, bool> predicate)
        {
            var attributes = await GetAllAsync();
            return attributes.Count(predicate);
        }

        public override async Task DeleteAsync(CategoryMandatoryAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.CategoryMandatoryAttribute WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.CategoryMandatoryAttribute WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<CategoryMandatoryAttribute>> FindAsync(Func<CategoryMandatoryAttribute, bool> predicate)
        {
            var attributes = await GetAllAsync();
            return attributes.Where(predicate);
        }

        public override async Task<IEnumerable<CategoryMandatoryAttribute>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryAttribute ORDER BY CASE WHEN SortOrder IS NULL THEN 1 ELSE 0 END, SortOrder, Name_en";
            return await dbConnection.QueryAsync<CategoryMandatoryAttribute>(query);
        }

        public override async Task<CategoryMandatoryAttribute> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryAttribute WHERE Id = @id";
            var result = await dbConnection.QueryFirstOrDefaultAsync<CategoryMandatoryAttribute>(query, new { id });
            
            return result ?? throw new InvalidOperationException($"CategoryMandatoryAttribute with id {id} not found");
        }

        public override async Task<CategoryMandatoryAttribute> UpdateAsync(CategoryMandatoryAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.CategoryMandatoryAttribute 
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

        public async Task<IEnumerable<CategoryMandatoryAttribute>> GetAttributesByCategoryNodeIdAsync(Guid categoryNodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryAttribute WHERE CategoryNodeId = @categoryNodeId ORDER BY CASE WHEN SortOrder IS NULL THEN 1 ELSE 0 END, SortOrder, Name_en";
            return await dbConnection.QueryAsync<CategoryMandatoryAttribute>(query, new { categoryNodeId });
        }

        public async Task<bool> DeleteAttributesByCategoryNodeIdAsync(Guid categoryNodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.CategoryMandatoryAttribute WHERE CategoryNodeId = @categoryNodeId";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { categoryNodeId });
            
            return rowsAffected > 0;
        }
    }
}
