using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class LoginService(IUserRepository userRepository, IEmailService emailService, ISessionService sessionService, IUserService userService) : ILoginService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IEmailService _emailService = emailService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly IUserService _userService = userService;

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string? userAgent = null, string? ipAddress = null)
        {
            var validationResult = request.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<LoginResponse>(validationResult.Error ?? "Validation failed.", validationResult.ErrorCode ?? StatusCodes.Status400BadRequest);
            }
            var foundUser = await _userRepository.FindByEmailAsync(request.Email);
            if (foundUser == null)
            {
                return Result.Failure<LoginResponse>("Invalid email or password", StatusCodes.Status401Unauthorized);
            }
            if (foundUser.Deleted)
            {
                return Result.Failure<LoginResponse>("User account is deleted", StatusCodes.Status401Unauthorized);
            }
            
            if (!foundUser.ValidEmail)
            {
                return Result.Failure<LoginResponse>("Please validate your email address before logging in", StatusCodes.Status403Forbidden);
            }
            
            // Check if account is locked due to failed login attempts
            if (foundUser.FailedLoginAttempts >= 3 && foundUser.LastFailedLoginAttempt.HasValue)
            {
                var lockoutExpiry = foundUser.LastFailedLoginAttempt.Value.AddMinutes(10);
                if (DateTime.UtcNow < lockoutExpiry)
                {
                    var remainingMinutes = (lockoutExpiry - DateTime.UtcNow).TotalMinutes;
                    return Result.Failure<LoginResponse>(
                        $"Account is locked due to too many failed login attempts. Please try again in {Math.Ceiling(remainingMinutes)} minute(s).", 
                        StatusCodes.Status429TooManyRequests);
                }
            }
            var hasher = new PasswordHasher();
            if (string.IsNullOrEmpty(request.Password) || !hasher.VerifyPassword(request.Password, foundUser.Password))
            {
                // Increment failed login attempts
                // TODO: Consider implementing optimistic concurrency control to handle race conditions
                // when multiple simultaneous login attempts occur for the same user account.
                // This could be done by adding a version/timestamp field to User entity.
                foundUser.FailedLoginAttempts++;
                foundUser.LastFailedLoginAttempt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(foundUser);
                
                return Result.Failure<LoginResponse>("Invalid email or password", StatusCodes.Status401Unauthorized);
            }
            
            // Reset failed login attempts on successful login
            if (foundUser.FailedLoginAttempts > 0)
            {
                foundUser.FailedLoginAttempts = 0;
                foundUser.LastFailedLoginAttempt = null;
                await _userRepository.UpdateAsync(foundUser);
            }
            
            // Login successful - create a new session
            var sessionResult = await _sessionService.CreateSessionAsync(foundUser.ID, userAgent, ipAddress);
            if (sessionResult.IsFailure)
            {
                return Result.Failure<LoginResponse>("Login successful but failed to create session", StatusCodes.Status500InternalServerError);
            }

            return Result.Success(new LoginResponse 
            { 
                SessionId = sessionResult.Value?.SessionId 
            });
        }

        public async Task<Result<bool>> SendValidationEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<bool>("Email is required.", StatusCodes.Status400BadRequest);
            }

            var user = await _userRepository.FindByEmailAsync(email);
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
                // Generate a new validation token
                user.EmailValidationToken = GenerateSecureToken();
                user.Lastupdatedat = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                var emailSent = await _emailService.SendEmailValidationAsync(user);
                if (emailSent.IsFailure)
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

        private static string GenerateSecureToken()
        {
            // Generate a cryptographically secure random token
            const int tokenLength = 32; // 256 bits
            var tokenBytes = new byte[tokenLength];
            System.Security.Cryptography.RandomNumberGenerator.Fill(tokenBytes);
            return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}