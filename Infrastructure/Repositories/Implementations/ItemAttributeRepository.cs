using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class ItemAttributeRepository(string connectionString) : GenericRepository<ItemAttribute>(connectionString), IItemAttributeRepository
    {
        public override async Task<ItemAttribute> AddAsync(ItemAttribute entity)
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
INSERT INTO dbo.ItemAttribute (Id, ItemVariantID, AttributeName_en, AttributeName_fr, Attributes_en, Attributes_fr)
VALUES (@Id, @ItemVariantID, @AttributeName_en, @AttributeName_fr, @Attributes_en, @Attributes_fr)";

            var parameters = new
            {
                entity.Id,
                entity.ItemVariantID,
                entity.AttributeName_en,
                entity.AttributeName_fr,
                entity.Attributes_en,
                entity.Attributes_fr
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override async Task<int> CountAsync(Func<ItemAttribute, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Count(predicate);
        }

        public override async Task DeleteAsync(ItemAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.ItemAttribute WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.ItemAttribute WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<ItemAttribute>> FindAsync(Func<ItemAttribute, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Where(predicate);
        }

        public override async Task<IEnumerable<ItemAttribute>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemAttribute";
            return await dbConnection.QueryAsync<ItemAttribute>(query);
        }

        public override async Task<ItemAttribute> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemAttribute WHERE Id = @id";
            var result = await dbConnection.QueryFirstOrDefaultAsync<ItemAttribute>(query, new { id });
            
            return result ?? throw new InvalidOperationException($"ItemAttribute with id {id} not found");
        }

        public override async Task<ItemAttribute> UpdateAsync(ItemAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.ItemAttribute 
SET ItemVariantID = @ItemVariantID, AttributeName_en = @AttributeName_en, AttributeName_fr = @AttributeName_fr, 
    Attributes_en = @Attributes_en, Attributes_fr = @Attributes_fr
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.ItemVariantID,
                entity.AttributeName_en,
                entity.AttributeName_fr,
                entity.Attributes_en,
                entity.Attributes_fr
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public async Task<IEnumerable<ItemAttribute>> GetAttributesByItemVariantIdAsync(Guid itemVariantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemAttribute WHERE ItemVariantID = @itemVariantId";
            return await dbConnection.QueryAsync<ItemAttribute>(query, new { itemVariantId });
        }

        public async Task<bool> DeleteAttributesByItemVariantIdAsync(Guid itemVariantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.ItemAttribute WHERE ItemVariantID = @itemVariantId";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { itemVariantId });
            
            return rowsAffected > 0;
        }
    }
}