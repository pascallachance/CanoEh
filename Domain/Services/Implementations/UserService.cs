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

        public async Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest newUser)
        {
            var validationResult = newUser.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<CreateUserResponse>(validationResult.Error, validationResult.ErrorCode ?? StatusCodes.Status400BadRequest);
            }

            var existingUser = await Task.Run(() => _userRepository.Find(u => u.Uname == newUser.Uname).FirstOrDefault());
            if (existingUser != null)
            {
                return Result.Failure<CreateUserResponse>("Username already exists.", StatusCodes.Status400BadRequest);
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

        public async Task<Result<GetUserResponse>> GetUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Result.Failure<GetUserResponse>("Username is required.", StatusCodes.Status400BadRequest);
            }
            var userFound = await Task.Run(() => _userRepository.Find(u => u.Uname == username).FirstOrDefault());
            if (userFound == null)
            {
                return Result.Failure<GetUserResponse>("User not found.", StatusCodes.Status404NotFound);
            }

            GetUserResponse userResponse = userFound.ConvertToGetUserResponse();
            return Result.Success(userResponse);
        }

        public async Task<Result<UpdateUserResponse>> UpdateUserAsync(UpdateUserRequest updateRequest)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(updateRequest.UserName))
            {
                return Result.Failure<UpdateUserResponse>("Username is required.", StatusCodes.Status400BadRequest);
            }

            var validationResult = updateRequest.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<UpdateUserResponse>(validationResult.Error, validationResult.ErrorCode ?? StatusCodes.Status400BadRequest);
            }

            // Find the user to update
            var userToUpdate = await Task.Run(() => _userRepository.Find(u => u.Uname == updateRequest.UserName).FirstOrDefault());
            if (userToUpdate == null)
            {
                return Result.Failure<UpdateUserResponse>("User not found.", StatusCodes.Status404NotFound);
            }

            // Update only the allowed fields
            userToUpdate.Firstname = updateRequest.Firstname;
            userToUpdate.Lastname = updateRequest.Lastname;
            userToUpdate.Email = updateRequest.Email;
            userToUpdate.Phone = updateRequest.Phone;
            userToUpdate.Lastupdatedat = DateTime.UtcNow; // Update LastUpdatedAt as required

            // Save changes
            var updatedUser = await Task.Run(() => _userRepository.Update(userToUpdate));

            // Convert to response model
            UpdateUserResponse response = updatedUser.ConvertToUpdateUserResponse();

            Debug.WriteLine($"User {updateRequest.UserName} updated successfully.");
            return Result.Success(response);
        }
    }
} 