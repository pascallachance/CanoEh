using System.Diagnostics;
using System.Net.Mail;
using Domain.Models.Converters;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class UserService(IRepository<User> userRepository, IEmailService emailService) : IUserService
    {
        private readonly IRepository<User> _userRepository = userRepository;
        private readonly IEmailService _emailService = emailService;

        public async Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest newUser)
        {
            var validationResult = newUser.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<CreateUserResponse>(
                    validationResult.Error ?? "Validation failed.", 
                    validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                );
            }

            var existingUsers = await _userRepository.FindAsync(u => u.Uname == newUser.Username);
            var existingUser = existingUsers.FirstOrDefault();
            if (existingUser != null)
            {
                return Result.Failure<CreateUserResponse>("Username already exists.", StatusCodes.Status400BadRequest);
            }

            var hasher = new PasswordHasher();
            var user = await _userRepository.AddAsync(new User
            {
                Uname = newUser.Username,
                Firstname = newUser.Firstname,
                Lastname = newUser.Lastname,
                Email = newUser.Email,
                Phone = newUser.Phone, 
                Lastlogin = null,
                Createdat = DateTime.UtcNow,
                Lastupdatedat = null,
                Password = hasher.HashPassword(newUser.Password),
                Deleted = false,
                ValidEmail = false
            });

            // Send email validation
            try
            {
                await _emailService.SendEmailValidationAsync(user.Email, user.Uname, user.ID);
                Debug.WriteLine($"Validation email sent to {user.Email}");
            }
            catch (SmtpException smtpEx)
            {
                Debug.WriteLine($"SMTP error while sending validation email: {smtpEx.Message}");
                // Continue with user creation even if email fails
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"HTTP error while sending validation email: {httpEx.Message}");
                // Continue with user creation even if email fails
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error while sending validation email: {ex.Message}");
                // Continue with user creation even if email fails
            }

            CreateUserResponse createdUser = UserConverters.ConvertToCreateUserResponse(user);

            Debug.WriteLine($"User {newUser.Username} created successfully.");
            return Result.Success(createdUser);
        }

        public async Task<Result<GetUserResponse>> GetUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Result.Failure<GetUserResponse>("Username is required.", StatusCodes.Status400BadRequest);
            }
            var users = await _userRepository.FindAsync(u => u.Uname == username);
            var userFound = users.FirstOrDefault();
            if (userFound == null)
            {
                return Result.Failure<GetUserResponse>("User not found.", StatusCodes.Status404NotFound);
            }
            if (userFound.Deleted)
            {
                return Result.Failure<GetUserResponse>("User is deleted.", StatusCodes.Status410Gone);
            }
            GetUserResponse userResponse = userFound.ConvertToGetUserResponse();
            return Result.Success(userResponse);
        }

        public async Task<Result<UpdateUserResponse>> UpdateUserAsync(UpdateUserRequest updateRequest)
        {
            // Validate input
             if (string.IsNullOrWhiteSpace(updateRequest.Username))
            {
                return Result.Failure<UpdateUserResponse>("Username is required.", StatusCodes.Status400BadRequest);
            }

            var validationResult = updateRequest.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<UpdateUserResponse>(validationResult.Error ?? "Invalid update data.", validationResult.ErrorCode ?? StatusCodes.Status400BadRequest);
            }

            // Find the user to update
            var users = await _userRepository.FindAsync(u => u.Uname == updateRequest.Username);
            var userToUpdate = users.FirstOrDefault();
            if (userToUpdate == null)
            {
                return Result.Failure<UpdateUserResponse>("User not found.", StatusCodes.Status404NotFound);
            }
            if (userToUpdate.Deleted)
            {
                return Result.Failure<UpdateUserResponse>("User is deleted.", StatusCodes.Status410Gone);
            }
            // Update only the allowed fields
            userToUpdate.Firstname = updateRequest.Firstname;
            userToUpdate.Lastname = updateRequest.Lastname;
            userToUpdate.Email = updateRequest.Email;
            userToUpdate.Phone = updateRequest.Phone;
            userToUpdate.Lastupdatedat = DateTime.UtcNow; // Update LastUpdatedAt as required

            // Save changes
            var updatedUser = await _userRepository.UpdateAsync(userToUpdate);

            // Convert to response model
            UpdateUserResponse response = updatedUser.ConvertToUpdateUserResponse();

            Debug.WriteLine($"User {updateRequest.Username} updated successfully.");
            return Result.Success(response);
        }

        public async Task<Result<DeleteUserResponse>> DeleteUserAsync(string username)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username))
            {
                return Result.Failure<DeleteUserResponse>("Username is required.", StatusCodes.Status400BadRequest);
            }

            // Find the user to delete
            var users = await _userRepository.FindAsync(u => u.Uname == username);
            var userToDelete = users.FirstOrDefault();
            if (userToDelete == null)
            {
                return Result.Failure<DeleteUserResponse>("User not found.", StatusCodes.Status404NotFound);
            }
            if (userToDelete.Deleted)
            {
                return Result.Failure<DeleteUserResponse>("User is already deleted.", StatusCodes.Status400BadRequest);
            }

            // Perform soft delete
            userToDelete.Deleted = true;
            userToDelete.Lastupdatedat = DateTime.UtcNow;

            // Save changes
            var deletedUser = await _userRepository.UpdateAsync(userToDelete);

            // Convert to response model
            DeleteUserResponse response = deletedUser.ConvertToDeleteUserResponse();

            Debug.WriteLine($"User {username} deleted successfully.");
            return Result.Success(response);
        }

        public async Task<Result<bool>> ValidateEmailAsync(Guid userId)
        {
            // Find the user by ID
            var users = await _userRepository.FindAsync(u => u.ID == userId);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return Result.Failure<bool>("User not found.", StatusCodes.Status404NotFound);
            }
            if (user.Deleted)
            {
                return Result.Failure<bool>("User is deleted.", StatusCodes.Status410Gone);
            }
            if (user.ValidEmail)
            {
                return Result.Failure<bool>("Email is already validated.", StatusCodes.Status400BadRequest);
            }

            // Mark email as validated
            user.ValidEmail = true;
            user.Lastupdatedat = DateTime.UtcNow;

            // Save changes
            await _userRepository.UpdateAsync(user);

            Debug.WriteLine($"Email validated for user {user.Uname}");
            return Result.Success(true);
        }

        public async Task<Result> UpdateLastLoginAsync (string username)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(username))
            {
                return Result.Failure("Username is required.", StatusCodes.Status400BadRequest);
            }
            // Find the user to update
            var users = await _userRepository.FindAsync(u => u.Uname == username);
            var userToUpdate = users.FirstOrDefault();
            if (userToUpdate == null)
            {
                return Result.Failure("User not found.", StatusCodes.Status404NotFound);
            }
            if (userToUpdate.Deleted)
            {
                return Result.Failure("User is deleted.", StatusCodes.Status410Gone);
            }
            // Update LastLogin field
            userToUpdate.Lastlogin = DateTime.UtcNow;
            // Save changes
            await _userRepository.UpdateAsync(userToUpdate);
            Debug.WriteLine($"Last login updated for user {username}");
            return Result.Success();
        }
    }
} 