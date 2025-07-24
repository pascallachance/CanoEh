using Domain.Models;
using Helpers.Common;
using Infrastructure.Data;

namespace Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result> CreateUserAsync(CreateUserRequest newUser);

        Task<Result<User>> GetUserAsync(string username);
    }
}