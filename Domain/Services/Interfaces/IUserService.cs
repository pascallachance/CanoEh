using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest newUser);

        Task<Result<GetUserResponse>> GetUserAsync(string username);

        Task<Result<DeleteUserResponse>> DeleteUserAsync(string username);

        Task<Result<UpdateUserResponse>> UpdateUserAsync(UpdateUserRequest updateRequest);

        Task<Result<bool>> ValidateEmailAsync(Guid userId);

        Task<Result<bool>> ValidateEmailByTokenAsync(string token);

        Task<Result<bool>> UpdateLastLoginAsync(string username);

        Task<Result<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest);
    }
}