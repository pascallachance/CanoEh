using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class CategoryMandatoryFeatureRepository(string connectionString) : GenericRepository<CategoryMandatoryFeature>(connectionString), ICategoryMandatoryFeatureRepository
    {
        public override async Task<CategoryMandatoryFeature> AddAsync(CategoryMandatoryFeature entity)
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

        public override async Task<int> CountAsync(Func<CategoryMandatoryFeature, bool> predicate)
        {
            var features = await GetAllAsync();
            return features.Count(predicate);
        }

        public override async Task DeleteAsync(CategoryMandatoryFeature entity)
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

        public override async Task<IEnumerable<CategoryMandatoryFeature>> FindAsync(Func<CategoryMandatoryFeature, bool> predicate)
        {
            var features = await GetAllAsync();
            return features.Where(predicate);
        }

        public override async Task<IEnumerable<CategoryMandatoryFeature>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryAttribute ORDER BY CASE WHEN SortOrder IS NULL THEN 1 ELSE 0 END, SortOrder, Name_en";
            return await dbConnection.QueryAsync<CategoryMandatoryFeature>(query);
        }

        public override async Task<CategoryMandatoryFeature> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryAttribute WHERE Id = @id";
            var result = await dbConnection.QueryFirstOrDefaultAsync<CategoryMandatoryFeature>(query, new { id });
            
            return result ?? throw new InvalidOperationException($"CategoryMandatoryFeature with id {id} not found");
        }

        public override async Task<CategoryMandatoryFeature> UpdateAsync(CategoryMandatoryFeature entity)
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

        public async Task<IEnumerable<CategoryMandatoryFeature>> GetFeaturesByCategoryNodeIdAsync(Guid categoryNodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.CategoryMandatoryAttribute WHERE CategoryNodeId = @categoryNodeId ORDER BY CASE WHEN SortOrder IS NULL THEN 1 ELSE 0 END, SortOrder, Name_en";
            return await dbConnection.QueryAsync<CategoryMandatoryFeature>(query, new { categoryNodeId });
        }

        public async Task<bool> DeleteFeaturesByCategoryNodeIdAsync(Guid categoryNodeId)
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
