using Domain.Models;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class LoginService
    {
        private readonly IRepository<User> _userRepository;

        public LoginService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> LoginAsync(LoginRequest request)
        {
            var validationResult = request.Validate();
            if (validationResult.IsFailure)
            {
                return validationResult;
            }
            var foundUser = await Task.Run(() => _userRepository.Find(u => u.Uname == request.Username).FirstOrDefault());
            if (foundUser == null)
            {
                return Result.Failure<CreateUserRequest>("Invalid username or password", StatusCodes.Status401Unauthorized);
            }

            var hasher = new PasswordHasher();
            if (string.IsNullOrEmpty(request.Password) || !hasher.VerifyPassword(request.Password, foundUser.Password))
            {
                return Result.Failure<CreateUserRequest>("Invalid username or password", StatusCodes.Status401Unauthorized);
            }
            return Result.Success();
        }
    }
}
