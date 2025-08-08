using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface IAddressService
    {
        Task<Result<CreateAddressResponse>> CreateAddressAsync(CreateAddressRequest request, Guid userId);
        
        Task<Result<UpdateAddressResponse>> UpdateAddressAsync(UpdateAddressRequest request, Guid userId);
        
        Task<Result<DeleteAddressResponse>> DeleteAddressAsync(Guid addressId, Guid userId);
        
        Task<Result<GetAddressResponse>> GetAddressAsync(Guid addressId, Guid userId);
        
        Task<Result<IEnumerable<GetAddressResponse>>> GetUserAddressesAsync(Guid userId);
        
        Task<Result<IEnumerable<GetAddressResponse>>> GetUserAddressesByTypeAsync(Guid userId, string addressType);
    }
}