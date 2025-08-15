using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class OrderPaymentRepository(string connectionString) : GenericRepository<OrderPayment>(connectionString), IOrderPaymentRepository
    {
        public override async Task<OrderPayment> AddAsync(OrderPayment entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.OrderPayment (
    ID,
    OrderID,
    PaymentMethodID,
    Amount,
    Provider,
    ProviderReference,
    PaidAt)
VALUES (
    @ID,
    @OrderID,
    @PaymentMethodID,
    @Amount,
    @Provider,
    @ProviderReference,
    @PaidAt)";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task<OrderPayment> UpdateAsync(OrderPayment entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderPayment SET
    PaymentMethodID = @PaymentMethodID,
    Amount = @Amount,
    Provider = @Provider,
    ProviderReference = @ProviderReference,
    PaidAt = @PaidAt
WHERE ID = @ID";

            await dbConnection.ExecuteAsync(query, entity);
            return entity;
        }

        public override async Task DeleteAsync(OrderPayment entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "DELETE FROM dbo.OrderPayment WHERE ID = @ID";
            await dbConnection.ExecuteAsync(query, new { entity.ID });
        }

        public override async Task<OrderPayment> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderPayment WHERE ID = @id";
            return await dbConnection.QuerySingleAsync<OrderPayment>(query, new { id });
        }

        public override async Task<IEnumerable<OrderPayment>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderPayment";
            return await dbConnection.QueryAsync<OrderPayment>(query);
        }

        public override async Task<IEnumerable<OrderPayment>> FindAsync(Func<OrderPayment, bool> predicate)
        {
            var allOrderPayments = await GetAllAsync();
            return allOrderPayments.Where(predicate);
        }

        public override async Task<int> CountAsync(Func<OrderPayment, bool> predicate)
        {
            var allOrderPayments = await GetAllAsync();
            return allOrderPayments.Count(predicate);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT COUNT(1) FROM dbo.OrderPayment WHERE ID = @id";
            var count = await dbConnection.QuerySingleAsync<int>(query, new { id });
            return count > 0;
        }

        public async Task<OrderPayment?> FindByOrderIdAsync(Guid orderId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = "SELECT * FROM dbo.OrderPayment WHERE OrderID = @orderId";
            return await dbConnection.QuerySingleOrDefaultAsync<OrderPayment>(query, new { orderId });
        }

        public async Task<bool> UpdatePaidStatusAsync(Guid orderPaymentId, DateTime paidAt, string? providerReference)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.OrderPayment SET
    PaidAt = @paidAt,
    ProviderReference = @providerReference
WHERE ID = @orderPaymentId";

            var rowsAffected = await dbConnection.ExecuteAsync(query, new { orderPaymentId, paidAt, providerReference });
            return rowsAffected > 0;
        }
    }
}