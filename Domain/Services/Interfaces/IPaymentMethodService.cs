using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<Result<CreatePaymentMethodResponse>> CreatePaymentMethodAsync(Guid userId, CreatePaymentMethodRequest createRequest);
        Task<Result<GetPaymentMethodResponse>> GetPaymentMethodAsync(Guid userId, Guid paymentMethodId);
        Task<Result<IEnumerable<GetPaymentMethodResponse>>> GetUserPaymentMethodsAsync(Guid userId);
        Task<Result<IEnumerable<GetPaymentMethodResponse>>> GetActiveUserPaymentMethodsAsync(Guid userId);
        Task<Result<UpdatePaymentMethodResponse>> UpdatePaymentMethodAsync(Guid userId, UpdatePaymentMethodRequest updateRequest);
        Task<Result<DeletePaymentMethodResponse>> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId);
        Task<Result<GetPaymentMethodResponse>> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId);
        Task<Result<GetPaymentMethodResponse>> GetDefaultPaymentMethodAsync(Guid userId);
    }
}