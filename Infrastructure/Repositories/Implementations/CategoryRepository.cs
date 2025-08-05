using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class CategoryRepository(string connectionString) : GenericRepository<Category>(connectionString), ICategoryRepository
    {
        public override async Task<Category> AddAsync(Category entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
INSERT INTO dbo.Categories (Name_en, Name_fr, ParentCategoryId)
OUTPUT INSERTED.Id
VALUES (@Name_en, @Name_fr, @ParentCategoryId)";

            var parameters = new
            {
                entity.Name_en,
                entity.Name_fr,
                entity.ParentCategoryId
            };

            Guid newCategoryId = await dbConnection.ExecuteScalarAsync<Guid>(query, parameters);
            entity.Id = newCategoryId;

            return entity;
        }

        public override async Task<int> CountAsync(Func<Category, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var categories = await GetAllAsync();
            return categories.Count(predicate);
        }

        public override async Task DeleteAsync(Category entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Check if category has subcategories or items
            var hasSubcategories = await HasSubcategoriesAsync(entity.Id);
            var hasItems = await HasItemsAsync(entity.Id);

            if (hasSubcategories)
            {
                throw new InvalidOperationException("Cannot delete category that has subcategories.");
            }

            if (hasItems)
            {
                throw new InvalidOperationException("Cannot delete category that has items.");
            }

            var query = "DELETE FROM dbo.Categories WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Categories WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<Category>> FindAsync(Func<Category, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var categories = await GetAllAsync();
            return categories.Where(predicate);
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, ParentCategoryId
FROM dbo.Categories
ORDER BY Name_en";

            var categoriesData = await dbConnection.QueryAsync<CategoryDto>(query);
            
            return categoriesData.Select(dto => new Category
            {
                Id = dto.Id,
                Name_en = dto.Name_en,
                Name_fr = dto.Name_fr,
                ParentCategoryId = dto.ParentCategoryId
            });
        }

        public override async Task<Category> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var category = await GetCategoryByIdAsync(id);
            return category ?? throw new InvalidOperationException($"Category with id {id} not found");
        }

        public override async Task<Category> UpdateAsync(Category entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Validate that ParentCategoryId doesn't create a circular reference
            if (entity.ParentCategoryId.HasValue)
            {
                await ValidateNoCircularReferenceAsync(entity.Id, entity.ParentCategoryId.Value);
            }

            var query = @"
UPDATE dbo.Categories
SET Name_en = @Name_en,
    Name_fr = @Name_fr,
    ParentCategoryId = @ParentCategoryId
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.Name_en,
                entity.Name_fr,
                entity.ParentCategoryId
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        // ICategoryRepository specific methods
        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, ParentCategoryId
FROM dbo.Categories
WHERE Id = @id";

            var dto = await dbConnection.QueryFirstOrDefaultAsync<CategoryDto>(query, new { id });
            
            if (dto == null)
                return null;

            return new Category
            {
                Id = dto.Id,
                Name_en = dto.Name_en,
                Name_fr = dto.Name_fr,
                ParentCategoryId = dto.ParentCategoryId
            };
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, ParentCategoryId
FROM dbo.Categories
WHERE ParentCategoryId IS NULL
ORDER BY Name_en";

            var categoriesData = await dbConnection.QueryAsync<CategoryDto>(query);
            
            return categoriesData.Select(dto => new Category
            {
                Id = dto.Id,
                Name_en = dto.Name_en,
                Name_fr = dto.Name_fr,
                ParentCategoryId = dto.ParentCategoryId
            });
        }

        public async Task<IEnumerable<Category>> GetSubcategoriesAsync(Guid parentCategoryId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, ParentCategoryId
FROM dbo.Categories
WHERE ParentCategoryId = @parentCategoryId
ORDER BY Name_en";

            var categoriesData = await dbConnection.QueryAsync<CategoryDto>(query, new { parentCategoryId });
            
            return categoriesData.Select(dto => new Category
            {
                Id = dto.Id,
                Name_en = dto.Name_en,
                Name_fr = dto.Name_fr,
                ParentCategoryId = dto.ParentCategoryId
            });
        }

        public async Task<bool> HasSubcategoriesAsync(Guid categoryId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = "SELECT COUNT(1) FROM dbo.Categories WHERE ParentCategoryId = @categoryId";
            var count = await dbConnection.ExecuteScalarAsync<int>(query, new { categoryId });
            return count > 0;
        }

        public async Task<bool> HasItemsAsync(Guid categoryId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Note: This assumes we'll update Item table to use CategoryId instead of Category string
            // For now, this will always return false until the Item model is updated
            var query = "SELECT COUNT(1) FROM dbo.Items WHERE CategoryId = @categoryId AND Deleted = 0";
            var count = await dbConnection.ExecuteScalarAsync<int>(query, new { categoryId });
            return count > 0;
        }

        private async Task ValidateNoCircularReferenceAsync(Guid categoryId, Guid proposedParentId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Check if proposedParentId is a descendant of categoryId
            var query = @"
WITH CategoryAncestors AS (
    SELECT Id, ParentCategoryId
    FROM dbo.Categories
    WHERE Id = @proposedParentId
    
    UNION ALL
    
    SELECT c.Id, c.ParentCategoryId
    FROM dbo.Categories c
    INNER JOIN CategoryAncestors ca ON c.Id = ca.ParentCategoryId
)
SELECT COUNT(1)
FROM CategoryAncestors
WHERE Id = @categoryId";

            var count = await dbConnection.ExecuteScalarAsync<int>(query, new { categoryId, proposedParentId });
            
            if (count > 0)
            {
                throw new InvalidOperationException("Setting this parent would create a circular reference.");
            }
        }

        private sealed class CategoryDto
        {
            public Guid Id { get; set; }
            public string Name_en { get; set; } = string.Empty;
            public string Name_fr { get; set; } = string.Empty;
            public Guid? ParentCategoryId { get; set; }
        }
    }
}