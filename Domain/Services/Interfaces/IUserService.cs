using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;
using Infrastructure.Data;

namespace Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest newUser);

        Task<Result<GetUserResponse>> GetUserAsync(string email);
        
        Task<Result<User?>> GetUserEntityAsync(string email);

        Task<Result<DeleteUserResponse>> DeleteUserAsync(string email);

        Task<Result<UpdateUserResponse>> UpdateUserAsync(UpdateUserRequest updateRequest);

        Task<Result<bool>> ValidateEmailAsync(Guid userId);

        Task<Result<bool>> ValidateEmailByTokenAsync(string token);

        Task<Result<bool>> UpdateLastLoginAsync(string email);

        Task<Result<bool>> LogoutAsync(string email);

        Task<Result<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest);

        Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest);

        Task<Result<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);

        Task<Result<SendRestoreUserEmailResponse>> SendRestoreUserEmailAsync(SendRestoreUserEmailRequest sendRestoreUserEmailRequest);

        Task<Result<RestoreUserResponse>> RestoreUserAsync(RestoreUserRequest restoreUserRequest);

        Task<Result<User?>> FindByRefreshTokenAsync(string refreshToken);

        Task<Result<bool>> UpdateRefreshTokenAsync(string email, string refreshToken, DateTime expiry);

        Task<Result<bool>> ClearRefreshTokenAsync(string email);
    }
}