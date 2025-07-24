using Domain.Models.Requests;
using Domain.Models.Responses;
using Helpers.Common;
using Infrastructure.Data;

namespace Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest newUser);

        Task<Result<GetUserResponse>> GetUserAsync(string username);

        Task<Result<UpdateUserResponse>> UpdateUserAsync(UpdateUserRequest updateRequest);
    }
}