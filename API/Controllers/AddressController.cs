using System.Diagnostics;
using System.Security.Claims;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController(IAddressService addressService, IUserService userService) : ControllerBase
    {
        private readonly IAddressService _addressService = addressService;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Creates a new address for the authenticated user.
        /// </summary>
        /// <param name="request">The address details to create.</param>
        /// <returns>Returns the created address or an error response.</returns>
        [HttpPost("CreateAddress")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateAddressResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = await GetUserIdFromTokenAsync();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user token.");
                }

                var result = await _addressService.CreateAddressAsync(request, userId.Value);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
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
        /// Updates an existing address for the authenticated user.
        /// </summary>
        /// <param name="request">The address details to update.</param>
        /// <returns>Returns the updated address or an error response.</returns>
        [HttpPut("UpdateAddress")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateAddressResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = await GetUserIdFromTokenAsync();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user token.");
                }

                var result = await _addressService.UpdateAddressAsync(request, userId.Value);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
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
        /// Deletes an address for the authenticated user.
        /// </summary>
        /// <param name="addressId">The ID of the address to delete.</param>
        /// <returns>Returns success message or an error response.</returns>
        [HttpDelete("DeleteAddress/{addressId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteAddressResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAddress(Guid addressId)
        {
            try
            {
                if (addressId == Guid.Empty)
                {
                    return BadRequest("Address ID is required.");
                }

                var userId = await GetUserIdFromTokenAsync();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user token.");
                }

                var result = await _addressService.DeleteAddressAsync(addressId, userId.Value);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
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
        /// Gets a specific address for the authenticated user.
        /// </summary>
        /// <param name="addressId">The ID of the address to retrieve.</param>
        /// <returns>Returns the address or an error response.</returns>
        [HttpGet("GetAddress/{addressId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAddressResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAddress(Guid addressId)
        {
            try
            {
                if (addressId == Guid.Empty)
                {
                    return BadRequest("Address ID is required.");
                }

                var userId = await GetUserIdFromTokenAsync();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user token.");
                }

                var result = await _addressService.GetAddressAsync(addressId, userId.Value);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
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
        /// Gets all addresses for the authenticated user.
        /// </summary>
        /// <returns>Returns the list of addresses or an error response.</returns>
        [HttpGet("GetUserAddresses")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetAddressResponse>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserAddresses()
        {
            try
            {
                var userId = await GetUserIdFromTokenAsync();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user token.");
                }

                var result = await _addressService.GetUserAddressesAsync(userId.Value);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
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
        /// Gets addresses of a specific type for the authenticated user.
        /// </summary>
        /// <param name="addressType">The type of address to retrieve (Delivery, Billing, Company).</param>
        /// <returns>Returns the list of addresses of the specified type or an error response.</returns>
        [HttpGet("GetUserAddressesByType/{addressType}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetAddressResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserAddressesByType(string addressType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(addressType))
                {
                    return BadRequest("Address type is required.");
                }

                var userId = await GetUserIdFromTokenAsync();
                if (userId == null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "Invalid user token.");
                }

                var result = await _addressService.GetUserAddressesByTypeAsync(userId.Value, addressType);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
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
        /// Helper method to get the user ID from the JWT token.
        /// </summary>
        /// <returns>The user ID or null if not found.</returns>
        private async Task<Guid?> GetUserIdFromTokenAsync()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    return null;
                }

                var userResult = await _userService.GetUserEntityAsync(email);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return null;
                }

                return userResult.Value.ID;
            }
            catch
            {
                return null;
            }
        }
    }
}