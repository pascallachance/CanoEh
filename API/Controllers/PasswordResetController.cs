using System.Diagnostics;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordResetController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Initiates a password reset process by sending a reset email to the user.
        /// </summary>
        /// <param name="forgotPasswordRequest">The request containing the user's email address.</param>
        /// <returns>Returns a success message regardless of whether the email exists to prevent enumeration attacks.</returns>
        [HttpPost("ForgotPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.ForgotPasswordAsync(forgotPasswordRequest);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets a user's password using a valid reset token.
        /// </summary>
        /// <param name="resetPasswordRequest">The request containing the reset token and new password.</param>
        /// <returns>Returns a success message if the password was reset successfully.</returns>
        [HttpPost("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.ResetPasswordAsync(resetPasswordRequest);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}