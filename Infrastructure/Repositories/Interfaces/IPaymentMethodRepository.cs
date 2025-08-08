using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod>
    {
        Task<IEnumerable<PaymentMethod>> FindByUserIdAsync(Guid userId);
        Task<PaymentMethod?> FindByUserIdAndIdAsync(Guid userId, Guid id);
        Task<PaymentMethod?> FindDefaultByUserIdAsync(Guid userId);
        Task<bool> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId);
        Task<bool> ClearDefaultPaymentMethodsAsync(Guid userId);
        Task<IEnumerable<PaymentMethod>> FindActiveByUserIdAsync(Guid userId);
        Task<bool> DeactivatePaymentMethodAsync(Guid id);
    }
}