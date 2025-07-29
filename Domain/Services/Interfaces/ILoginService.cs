using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface ILoginService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? userAgent = null, string? ipAddress = null);
        Task<Result<bool>> SendValidationEmailAsync(string username);
    }
}