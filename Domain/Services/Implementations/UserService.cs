using System.Diagnostics;
using System.Net.Mail;
using Domain.Models.Converters;
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
    public class UserService(IUserRepository userRepository, IEmailService emailService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IEmailService _emailService = emailService;

        public async Task<Result<CreateUserResponse>> CreateUserAsync(CreateUserRequest newUser)
        {
            User user = null; 
            try { 
                var validationResult = newUser.Validate();
                if (validationResult.IsFailure)
                {
                    return Result.Failure<CreateUserResponse>(
                        validationResult.Error ?? "Validation failed.", 
                        validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                    );
                }

                var existingUser = await _userRepository.FindByUsernameAsync(newUser.Username);
                if (existingUser != null)
                {
                    return Result.Failure<CreateUserResponse>("Username already exists.", StatusCodes.Status400BadRequest);
                }

                var hasher = new PasswordHasher();
                user = await _userRepository.AddAsync(new User
                {
                    Firstname = newUser.Firstname,
                    Lastname = newUser.Lastname,
                    Email = newUser.Email,
                    Phone = newUser.Phone, 
                    Lastlogin = null,
                    Createdat = DateTime.UtcNow,
                    Lastupdatedat = null,
                    Password = hasher.HashPassword(newUser.Password),
                    Deleted = false,
                    ValidEmail = false,
                    EmailValidationToken = GenerateSecureToken()
                });

                // Send email validation
                var result = await _emailService.SendEmailValidationAsync(user.Email, user.Email, user.EmailValidationToken!);
                if (result.IsFailure)
                {
                    Debug.WriteLine($"Email send failed: {result.Error}");
                }
                else
                {
                    Debug.WriteLine($"Validation email sent to {user.Email}");
                }
                    
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

            if (user == null)
            {
                return Result.Failure<CreateUserResponse>("Failed to create user.", StatusCodes.Status500InternalServerError);
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
            var userFound = await _userRepository.FindByUsernameAsync(username);
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

        public async Task<Result<User?>> GetUserEntityAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Result.Failure<User?>("Username is required.", StatusCodes.Status400BadRequest);
            }
            var userFound = await _userRepository.FindByUsernameAsync(username);
            if (userFound == null)
            {
                return Result.Success<User?>(null);
            }
            if (userFound.Deleted)
            {
                return Result.Success<User?>(null);
            }
            return Result.Success<User?>(userFound);
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
            var userToUpdate = await _userRepository.FindByUsernameAsync(updateRequest.Username);
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
            var userToDelete = await _userRepository.FindByUsernameAsync(username);
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
            var user = await _userRepository.GetByIdAsync(userId);
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

            Debug.WriteLine($"Email validated for user {user.Email}");
            return Result.Success(true);
        }

        public async Task<Result<bool>> UpdateLastLoginAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<bool>("Email is required.", StatusCodes.Status400BadRequest);
            }

            var userToUpdate = await _userRepository.FindByEmailAsync(email);
            if (userToUpdate == null)
            {
                return Result.Failure<bool>("User not found.", StatusCodes.Status404NotFound);
            }
            if (userToUpdate.Deleted)
            {
                return Result.Failure<bool>("User is deleted.", StatusCodes.Status410Gone);
            }

            userToUpdate.Lastlogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(userToUpdate);

            Debug.WriteLine($"Last login updated for user {email}");
            return Result.Success(true);
        }

        public async Task<Result<bool>> LogoutAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<bool>("Email is required.", StatusCodes.Status400BadRequest);
            }

            var userToUpdate = await _userRepository.FindByEmailAsync(email);
            if (userToUpdate == null)
            {
                return Result.Failure<bool>("User not found.", StatusCodes.Status404NotFound);
            }
            if (userToUpdate.Deleted)
            {
                return Result.Failure<bool>("User is deleted.", StatusCodes.Status410Gone);
            }

            userToUpdate.Lastlogout = DateTime.UtcNow;
            userToUpdate.Lastupdatedat = DateTime.UtcNow;
            await _userRepository.UpdateAsync(userToUpdate);

            Debug.WriteLine($"User {email} logged out successfully");
            return Result.Success(true);
        }

        public async Task<Result<bool>> ValidateEmailByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return Result.Failure<bool>("Validation token is required.", StatusCodes.Status400BadRequest);
            }

            // Find the user by validation token
            var user = await _userRepository.FindByEmailValidationTokenAsync(token);
            if (user == null)
            {
                return Result.Failure<bool>("Invalid or expired validation token.", StatusCodes.Status404NotFound);
            }
            if (user.Deleted)
            {
                return Result.Failure<bool>("User is deleted.", StatusCodes.Status410Gone);
            }
            if (user.ValidEmail)
            {
                return Result.Failure<bool>("Email is already validated.", StatusCodes.Status400BadRequest);
            }

            // Mark email as validated and clear the token
            user.ValidEmail = true;
            user.EmailValidationToken = null; // Clear the token after use
            user.Lastupdatedat = DateTime.UtcNow;

            // Save changes
            await _userRepository.UpdateAsync(user);

            Debug.WriteLine($"Email validated for user {user.Email}");
            return Result.Success(true);
        }

        public async Task<Result<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest)
        {
            // Validate input
            var validationResult = changePasswordRequest.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<ChangePasswordResponse>(
                    validationResult.Error ?? "Validation failed.", 
                    validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                );
            }

            // Find the user
            var user = await _userRepository.FindByUsernameAsync(changePasswordRequest.Username!);
            if (user == null)
            {
                return Result.Failure<ChangePasswordResponse>("User not found.", StatusCodes.Status404NotFound);
            }
            if (user.Deleted)
            {
                return Result.Failure<ChangePasswordResponse>("User is deleted.", StatusCodes.Status410Gone);
            }

            // Verify current password
            var passwordHasher = new PasswordHasher();
            if (!passwordHasher.VerifyPassword(changePasswordRequest.CurrentPassword!, user.Password))
            {
                return Result.Failure<ChangePasswordResponse>("Current password is incorrect.", StatusCodes.Status400BadRequest);
            }

            // Hash and update the new password
            user.Password = passwordHasher.HashPassword(changePasswordRequest.NewPassword!);
            user.Lastupdatedat = DateTime.UtcNow;

            // Save changes
            var updatedUser = await _userRepository.UpdateAsync(user);

            // Create response
            var response = new ChangePasswordResponse
            {
                Email = updatedUser.Email,
                LastUpdatedAt = updatedUser.Lastupdatedat ?? DateTime.UtcNow,
                Message = "Password changed successfully."
            };

            Debug.WriteLine($"Password changed successfully for user {changePasswordRequest.Username}");
            return Result.Success(response);
        }

        public async Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
        {
            var validationResult = forgotPasswordRequest.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<ForgotPasswordResponse>(
                    validationResult.Error ?? "Validation failed.", 
                    validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                );
            }

            // Find user by email - don't reveal if email exists or not for security
            var user = await _userRepository.FindByEmailAsync(forgotPasswordRequest.Email!);
            
            // Always return success response to prevent email enumeration attacks
            var response = new ForgotPasswordResponse
            {
                Email = forgotPasswordRequest.Email!,
                Message = "If the email address exists in our system, you will receive a password reset link shortly."
            };

            // Only send email if user exists and is not deleted
            if (user != null && !user.Deleted)
            {
                // Generate password reset token
                var resetToken = GenerateSecureToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

                // Update user with reset token
                var updateResult = await _userRepository.UpdatePasswordResetTokenAsync(user.Email, resetToken, tokenExpiry);
                if (updateResult)
                {
                    // Send password reset email
                    var emailResult = await _emailService.SendPasswordResetAsync(user.Email, user.Email, resetToken);
                    if (emailResult.IsFailure)
                    {
                        Debug.WriteLine($"Failed to send password reset email to {user.Email}: {emailResult.Error}");
                        // Don't return error to user for security reasons
                    }
                    else
                    {
                        Debug.WriteLine($"Password reset email sent to {user.Email}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Failed to update password reset token for user {user.Email}");
                    // Don't return error to user for security reasons
                }
            }
            else
            {
                Debug.WriteLine($"Password reset requested for non-existent or deleted email: {forgotPasswordRequest.Email}");
            }

            return Result.Success(response);
        }

        public async Task<Result<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var validationResult = resetPasswordRequest.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<ResetPasswordResponse>(
                    validationResult.Error ?? "Validation failed.", 
                    validationResult.ErrorCode ?? StatusCodes.Status400BadRequest
                );
            }

            // Find user by reset token
            var user = await _userRepository.FindByPasswordResetTokenAsync(resetPasswordRequest.Token!);
            if (user == null)
            {
                return Result.Failure<ResetPasswordResponse>("Invalid or expired reset token.", StatusCodes.Status400BadRequest);
            }
            if (user.Deleted)
            {
                return Result.Failure<ResetPasswordResponse>("User account is no longer active.", StatusCodes.Status410Gone);
            }

            // Hash the new password
            var hasher = new PasswordHasher();
            user.Password = hasher.HashPassword(resetPasswordRequest.NewPassword!);
            user.Lastupdatedat = DateTime.UtcNow;

            // Clear the reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            // Update user
            await _userRepository.UpdateAsync(user);

            // Create response
            var response = new ResetPasswordResponse
            {
                Message = "Password has been reset successfully.",
                ResetAt = DateTime.UtcNow
            };

            Debug.WriteLine($"Password reset successfully for user {user.Email}");
            return Result.Success(response);
        }

        public async Task<Result<SendRestoreUserEmailResponse>> SendRestoreUserEmailAsync(SendRestoreUserEmailRequest sendRestoreUserEmailRequest)
        {
            var validationResult = sendRestoreUserEmailRequest.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<SendRestoreUserEmailResponse>(validationResult.Error!, validationResult.ErrorCode!.Value);
            }

            try
            {
                // Find deleted user by email
                var deletedUser = await _userRepository.FindDeletedByEmailAsync(sendRestoreUserEmailRequest.Email!);
                
                if (deletedUser != null)
                {
                    // Generate restore token and set expiry (24 hours)
                    var restoreToken = GenerateSecureToken();
                    var tokenExpiry = DateTime.UtcNow.AddHours(24);

                    // Update user with restore token
                    var tokenUpdateResult = await _userRepository.UpdateRestoreUserTokenAsync(sendRestoreUserEmailRequest.Email!, restoreToken, tokenExpiry);
                    
                    if (tokenUpdateResult)
                    {
                        // Send restore email
                        var emailResult = await _emailService.SendRestoreUserEmailAsync(sendRestoreUserEmailRequest.Email!, deletedUser.Email, restoreToken);
                        
                        if (emailResult.IsFailure)
                        {
                            Debug.WriteLine($"Failed to send restore email: {emailResult.Error}");
                            // Don't expose email sending failures to the client for security
                        }
                    }
                }

                // Always return success for security reasons (don't reveal if email exists or not)
                var response = new SendRestoreUserEmailResponse
                {
                    Email = sendRestoreUserEmailRequest.Email!
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SendRestoreUserEmailAsync: {ex.Message}");
                return Result.Failure<SendRestoreUserEmailResponse>("An error occurred while processing the restore request.", 500);
            }
        }

        public async Task<Result<RestoreUserResponse>> RestoreUserAsync(RestoreUserRequest restoreUserRequest)
        {
            var validationResult = restoreUserRequest.Validate();
            if (validationResult.IsFailure)
            {
                return Result.Failure<RestoreUserResponse>(validationResult.Error!, validationResult.ErrorCode!.Value);
            }

            try
            {
                // Find deleted user by restore token
                var user = await _userRepository.FindByRestoreUserTokenAsync(restoreUserRequest.Token!);
                
                if (user == null)
                {
                    return Result.Failure<RestoreUserResponse>("Invalid or expired restore token.", 404);
                }

                // Restore the user (set deleted = false and clear token)
                var restoreResult = await _userRepository.RestoreUserByTokenAsync(restoreUserRequest.Token!);
                
                if (!restoreResult)
                {
                    return Result.Failure<RestoreUserResponse>("Failed to restore user account.", 500);
                }

                var response = new RestoreUserResponse
                {
                    Email = user.Email
                };

                Debug.WriteLine($"User {user.Email} has been successfully restored");
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RestoreUserAsync: {ex.Message}");
                return Result.Failure<RestoreUserResponse>("An error occurred while restoring the user account.", 500);
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