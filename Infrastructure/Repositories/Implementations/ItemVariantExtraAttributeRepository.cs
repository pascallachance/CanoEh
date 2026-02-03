using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class ItemVariantExtraAttributeRepository(string connectionString) : GenericRepository<ItemVariantExtraAttribute>(connectionString), IItemVariantExtraAttributeRepository
    {
        public override async Task<ItemVariantExtraAttribute> AddAsync(ItemVariantExtraAttribute entity)
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
INSERT INTO dbo.ItemVariantExtraAttribute (Id, ItemVariantId, Name_en, Name_fr, Value_en, Value_fr)
VALUES (@Id, @ItemVariantId, @Name_en, @Name_fr, @Value_en, @Value_fr)";

            var parameters = new
            {
                entity.Id,
                entity.ItemVariantId,
                entity.Name_en,
                entity.Name_fr,
                entity.Value_en,
                entity.Value_fr
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override async Task<int> CountAsync(Func<ItemVariantExtraAttribute, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Count(predicate);
        }

        public override async Task DeleteAsync(ItemVariantExtraAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.ItemVariantExtraAttribute WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.ItemVariantExtraAttribute WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<ItemVariantExtraAttribute>> FindAsync(Func<ItemVariantExtraAttribute, bool> predicate)
        {
            var items = await GetAllAsync();
            return items.Where(predicate);
        }

        public override async Task<IEnumerable<ItemVariantExtraAttribute>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariantExtraAttribute";
            return await dbConnection.QueryAsync<ItemVariantExtraAttribute>(query);
        }

        public override async Task<ItemVariantExtraAttribute> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariantExtraAttribute WHERE Id = @id";
            var result = await dbConnection.QueryFirstOrDefaultAsync<ItemVariantExtraAttribute>(query, new { id });
            
            return result ?? throw new InvalidOperationException($"ItemVariantExtraAttribute with id {id} not found");
        }

        public override async Task<ItemVariantExtraAttribute> UpdateAsync(ItemVariantExtraAttribute entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = @"
UPDATE dbo.ItemVariantExtraAttribute 
SET ItemVariantId = @ItemVariantId, Name_en = @Name_en, Name_fr = @Name_fr, 
    Value_en = @Value_en, Value_fr = @Value_fr
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.ItemVariantId,
                entity.Name_en,
                entity.Name_fr,
                entity.Value_en,
                entity.Value_fr
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public async Task<IEnumerable<ItemVariantExtraAttribute>> GetAttributesByVariantIdAsync(Guid variantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "SELECT * FROM dbo.ItemVariantExtraAttribute WHERE ItemVariantId = @variantId";
            return await dbConnection.QueryAsync<ItemVariantExtraAttribute>(query, new { variantId });
        }

        public async Task<bool> DeleteAttributesByVariantIdAsync(Guid variantId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            
            var query = "DELETE FROM dbo.ItemVariantExtraAttribute WHERE ItemVariantId = @variantId";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { variantId });
            
            return rowsAffected > 0;
        }
    }
}
