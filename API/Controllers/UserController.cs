using System.Diagnostics;
using System.Security.Claims;
using Domain.Models;
using Domain.Services.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="newUser">The user details to create.</param>
        /// <returns>Returns the created user or an error response.</returns>
        [HttpPost("CreateUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateUserRequest))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest newUser)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userService.CreateUserAsync(newUser);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the details of a user by their username.
        /// The user must be authenticated and can only access their own information.
        /// </summary>
        [Authorize]
        [HttpGet("GetUser/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetUserResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest("Username is required.");
                }

                var authenticatedUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.Equals(username, authenticatedUsername, StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "You can only access your own user information.");
                }

                var result = await _userService.GetUserAsync(username);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 404, result.Error);
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
        /// Updates the details of a user by their username.
        /// The user must be authenticated and can only update their own information.
        /// </summary>
        [Authorize]
        [HttpPut("UpdateUser/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateUserResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] UpdateUserRequest updateRequest)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest("Username is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate that the username in the URL matches the username in the request body
                if (!string.Equals(username, updateRequest.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Username in URL must match username in request body.");
                }

                var authenticatedUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Ensure user can only update their own information
                if (!string.Equals(username, authenticatedUsername, StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own user information.");
                }

                var result = await _userService.UpdateUserAsync(username, updateRequest);
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