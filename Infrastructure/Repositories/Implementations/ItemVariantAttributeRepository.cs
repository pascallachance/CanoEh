using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class ItemVariantAttributeRepository(string connectionString) : GenericRepository<ItemVariantAttribute>(connectionString), IItemVariantAttributeRepository
    {
        public override async Task<ItemVariantAttribute> AddAsync(ItemVariantAttribute entity)
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
INSERT INTO dbo.ItemVariantAttribute (Id, ItemVariantID, AttributeName_en, AttributeName_fr, Attributes_en, Attributes_fr)
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

        public override async Task<int> CountAsync(Func<ItemVariantAttribute, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Count(predicate);
        }

        public override async Task DeleteAsync(ItemVariantAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.ItemVariantAttribute WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.ItemVariantAttribute WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<ItemVariantAttribute>> FindAsync(Func<ItemVariantAttribute, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Where(predicate);
        }

        public override async Task<IEnumerable<ItemVariantAttribute>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariantAttribute";
            return await dbConnection.QueryAsync<ItemVariantAttribute>(query);
        }

        public override async Task<ItemVariantAttribute> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariantAttribute WHERE Id = @id";
            var result = await dbConnection.QueryFirstOrDefaultAsync<ItemVariantAttribute>(query, new { id });
            
            return result ?? throw new InvalidOperationException($"ItemVariantAttribute with id {id} not found");
        }

        public override async Task<ItemVariantAttribute> UpdateAsync(ItemVariantAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.ItemVariantAttribute 
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

        public async Task<IEnumerable<ItemVariantAttribute>> GetAttributesByVariantIdAsync(Guid variantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariantAttribute WHERE ItemVariantID = @variantId";
            return await dbConnection.QueryAsync<ItemVariantAttribute>(query, new { variantId });
        }

        public async Task<bool> DeleteAttributesByVariantIdAsync(Guid variantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.ItemVariantAttribute WHERE ItemVariantID = @variantId";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { variantId });
            
            return rowsAffected > 0;
        }
    }
}