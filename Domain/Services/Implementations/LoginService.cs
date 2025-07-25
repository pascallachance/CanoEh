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
    public class LoginService : ILoginService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IEmailService _emailService;

        public LoginService(IRepository<User> userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var validationResult = request.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(validationResult.Error ?? "Validation failed.", validationResult.ErrorCode ?? StatusCodes.Status400BadRequest);
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
            if (!foundUser.ValidEmail)
            {
                return Result.Failure<LoginResponse>("Please validate your email address before logging in", StatusCodes.Status403Forbidden);
            }
            var hasher = new PasswordHasher();
            if (string.IsNullOrEmpty(request.Password) || !hasher.VerifyPassword(request.Password, foundUser.Password))
            {
                return Result.Failure<LoginResponse>("Invalid username or password", StatusCodes.Status401Unauthorized);
            }
            
            // Login successful - return empty LoginResponse (token generation handled in controller)
            return Result.Success(new LoginResponse());
        }

        public async Task<Result<bool>> SendValidationEmailAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Result.Failure<bool>("Username is required.", StatusCodes.Status400BadRequest);
            }

            var user = await Task.Run(() => _userRepository.Find(u => u.Uname == username).FirstOrDefault());
            if (user == null)
            {
                return Result.Failure<bool>("User not found.", StatusCodes.Status404NotFound);
            }
            
            if (user.Deleted)
            {
                return Result.Failure<bool>("User account is deleted.", StatusCodes.Status400BadRequest);
            }
            
            if (user.ValidEmail)
            {
                return Result.Failure<bool>("Email is already validated.", StatusCodes.Status400BadRequest);
            }

            try
            {
                var emailSent = await _emailService.SendEmailValidationAsync(user.Email, user.Uname, user.ID);
                if (!emailSent)
                {
                    return Result.Failure<bool>("Failed to send validation email.", StatusCodes.Status500InternalServerError);
                }
                
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Error sending validation email: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }
    }
}