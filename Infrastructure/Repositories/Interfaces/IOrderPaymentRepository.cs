using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IOrderPaymentRepository : IRepository<OrderPayment>
    {
        Task<OrderPayment?> FindByOrderIdAsync(Guid orderId);
        Task<bool> UpdatePaidStatusAsync(Guid orderPaymentId, DateTime paidAt, string? providerReference);
    }
}