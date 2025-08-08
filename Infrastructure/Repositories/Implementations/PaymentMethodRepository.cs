using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class PaymentMethodRepository(string connectionString) : GenericRepository<PaymentMethod>(connectionString), IPaymentMethodRepository
    {
        public override async Task<PaymentMethod> AddAsync(PaymentMethod entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.PaymentMethod (
    UserID,
    Type,
    CardHolderName,
    CardLast4,
    CardBrand,
    ExpirationMonth,
    ExpirationYear,
    BillingAddress,
    IsDefault,
    CreatedAt,
    UpdatedAt,
    IsActive)
OUTPUT INSERTED.ID
VALUES (
    @UserID,
    @Type,
    @CardHolderName,
    @CardLast4,
    @CardBrand,
    @ExpirationMonth,
    @ExpirationYear,
    @BillingAddress,
    @IsDefault,
    @CreatedAt,
    @UpdatedAt,
    @IsActive)";

            var parameters = new
            {
                entity.UserID,
                entity.Type,
                entity.CardHolderName,
                entity.CardLast4,
                entity.CardBrand,
                entity.ExpirationMonth,
                entity.ExpirationYear,
                entity.BillingAddress,
                entity.IsDefault,
                entity.CreatedAt,
                entity.UpdatedAt,
                entity.IsActive
            };

            var id = await dbConnection.QuerySingleAsync<Guid>(query, parameters);
            entity.ID = id;
            return entity;
        }

        public override async Task<PaymentMethod> UpdateAsync(PaymentMethod entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.PaymentMethod 
SET 
    Type = @Type,
    CardHolderName = @CardHolderName,
    CardLast4 = @CardLast4,
    CardBrand = @CardBrand,
    ExpirationMonth = @ExpirationMonth,
    ExpirationYear = @ExpirationYear,
    BillingAddress = @BillingAddress,
    IsDefault = @IsDefault,
    UpdatedAt = @UpdatedAt,
    IsActive = @IsActive
WHERE ID = @ID";

            var parameters = new
            {
                entity.ID,
                entity.Type,
                entity.CardHolderName,
                entity.CardLast4,
                entity.CardBrand,
                entity.ExpirationMonth,
                entity.ExpirationYear,
                entity.BillingAddress,
                entity.IsDefault,
                entity.UpdatedAt,
                entity.IsActive
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override async Task DeleteAsync(PaymentMethod entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.PaymentMethod WHERE ID = @ID";
            await dbConnection.ExecuteAsync(query, new { entity.ID });
        }

        public override async Task<PaymentMethod> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.PaymentMethod WHERE ID = @id";
            var result = await dbConnection.QuerySingleOrDefaultAsync<PaymentMethod>(query, new { id });
            return result ?? throw new InvalidOperationException($"PaymentMethod with ID {id} not found.");
        }

        public override async Task<IEnumerable<PaymentMethod>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.PaymentMethod ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<PaymentMethod>(query);
        }

        public override async Task<IEnumerable<PaymentMethod>> FindAsync(Func<PaymentMethod, bool> predicate)
        {
            var allPaymentMethods = await GetAllAsync();
            return allPaymentMethods.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<PaymentMethod, bool> predicate)
        {
            var allPaymentMethods = await GetAllAsync();
            return allPaymentMethods.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.PaymentMethod WHERE ID = @id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { id });
            return count > 0;
        }

        public async Task<IEnumerable<PaymentMethod>> FindByUserIdAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.PaymentMethod WHERE UserID = @userId ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<PaymentMethod>(query, new { userId });
        }

        public async Task<PaymentMethod?> FindByUserIdAndIdAsync(Guid userId, Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.PaymentMethod WHERE UserID = @userId AND ID = @id";
            return await dbConnection.QuerySingleOrDefaultAsync<PaymentMethod>(query, new { userId, id });
        }

        public async Task<PaymentMethod?> FindDefaultByUserIdAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.PaymentMethod WHERE UserID = @userId AND IsDefault = 1 AND IsActive = 1";
            return await dbConnection.QuerySingleOrDefaultAsync<PaymentMethod>(query, new { userId });
        }

        public async Task<bool> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            using var transaction = dbConnection.BeginTransaction();
            try
            {
                // Clear all default flags for the user
                await ClearDefaultPaymentMethodsAsync(userId);
                
                // Set the specified payment method as default
                var query = @"
UPDATE dbo.PaymentMethod 
SET IsDefault = 1, UpdatedAt = @UpdatedAt 
WHERE UserID = @userId AND ID = @paymentMethodId AND IsActive = 1";
                
                var rowsAffected = await dbConnection.ExecuteAsync(query, 
                    new { userId, paymentMethodId, UpdatedAt = DateTime.UtcNow }, 
                    transaction);
                
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> ClearDefaultPaymentMethodsAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.PaymentMethod 
SET IsDefault = 0, UpdatedAt = @UpdatedAt 
WHERE UserID = @userId";
            
            var rowsAffected = await dbConnection.ExecuteAsync(query, 
                new { userId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<PaymentMethod>> FindActiveByUserIdAsync(Guid userId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.PaymentMethod WHERE UserID = @userId AND IsActive = 1 ORDER BY CreatedAt DESC";
            return await dbConnection.QueryAsync<PaymentMethod>(query, new { userId });
        }

        public async Task<bool> DeactivatePaymentMethodAsync(Guid userId, Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.PaymentMethod 
SET IsActive = 0, UpdatedAt = @UpdatedAt 
WHERE ID = @id AND UserID = @userId";
            
            var rowsAffected = await dbConnection.ExecuteAsync(query, 
                new { id, userId, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
    }
}