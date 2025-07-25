using Domain.Models.Requests;
using Domain.Models.Responses;
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
                return Result.Failure<LoginResponse>("Invalid username or password", StatusCodes.Status401Unauthorized);
            }
            if (foundUser.Deleted)
            {
                return Result.Failure<LoginResponse>("User account is deleted", StatusCodes.Status401Unauthorized);
            }
            var hasher = new PasswordHasher();
            if (string.IsNullOrEmpty(request.Password) || !hasher.VerifyPassword(request.Password, foundUser.Password))
            {
                return Result.Failure<LoginResponse>("Invalid username or password", StatusCodes.Status401Unauthorized);
            }
            return Result.Success();
        }
    }
}
