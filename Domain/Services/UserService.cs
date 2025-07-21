using System.Diagnostics;
using Domain.Models;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Domain.Services
{
    public class UserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result> CreateUserAsync(CreateUser newUser)
        {
            var validationResult = newUser.Validate();
            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            var existingUser = await Task.Run(() => _userRepository.Find(u => u.uname == newUser.uname).FirstOrDefault());
            if (existingUser != null)
            {
                return Result.Failure<CreateUser>("Username already exists.", StatusCodes.Status400BadRequest);
            }

            var hasher = new PasswordHasher();
            var user = await Task.Run(() => _userRepository.Add(new User
            {
                uname = newUser.uname,
                firstname = newUser.firstname,
                lastname = newUser.lastname,
                email = newUser.email,
                phone = newUser.phone,
                lastlogin = null,
                createdat = DateTime.UtcNow,
                lastupdatedat = null,
                password = hasher.HashPassword(newUser.password),
                deleted = false
            }));

            Debug.WriteLine($"User {newUser.uname} created successfully.");
            return Result.Success($"User {newUser.uname} created successfully.");
        }
    }
}
