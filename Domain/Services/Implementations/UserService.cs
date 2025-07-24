using System.Diagnostics;
using Domain.Models;
using Domain.Models.Converters;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class UserService(IRepository<User> userRepository) : IUserService
    {
        private readonly IRepository<User> _userRepository = userRepository;

        public async Task<Result> CreateUserAsync(CreateUserRequest newUser)
        {
            var validationResult = newUser.Validate();
            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            var existingUser = await Task.Run(() => _userRepository.Find(u => u.Uname == newUser.Uname).FirstOrDefault());
            if (existingUser != null)
            {
                return Result.Failure<CreateUserRequest>("Username already exists.", StatusCodes.Status400BadRequest);
            }

            var hasher = new PasswordHasher();
            var user = await Task.Run(() => _userRepository.Add(new User
            {
                Uname = newUser.Uname,
                Firstname = newUser.Firstname,
                Lastname = newUser.Lastname,
                Email = newUser.Email,
                Phone = newUser.Phone, 
                Lastlogin = null,
                Createdat = DateTime.UtcNow,
                Lastupdatedat = null,
                Password = hasher.HashPassword(newUser.Password),
                Deleted = false
            }));

            CreateUserResponse createdUser = UserConverters.ConvertToCreateUserResponse(user);

            Debug.WriteLine($"User {newUser.Uname} created successfully.");
            return Result.Success(createdUser);
        }

        public async Task<Result<User>> GetUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return (Result<User>)Result.Failure("Username is required.",StatusCodes.Status400BadRequest);
            }
            var user = await Task.Run(() => _userRepository.Find(u => u.Uname == username).FirstOrDefault());
            if (user == null)
            {
                return (Result<User>)Result.Failure("User not found.", StatusCodes.Status404NotFound);
            }
            return Result.Success(user);
        }
    }
} 
