using Domain.Models;
using Helpers.Common;
using Infrastructure.Data;

namespace Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest newUser);

        Task<Result<GetUserResponse>> GetUserAsync(string username);

        Task<Result<UpdateUserResponse>> UpdateUserAsync(string username, UpdateUserRequest updateRequest);
    }
}