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
    public class PaymentMethodController(IPaymentMethodService paymentMethodService, IUserService userService) : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService = paymentMethodService;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Creates a new payment method for the authenticated user.
        /// </summary>
        /// <param name="createRequest">The payment method details to create.</param>
        /// <returns>Returns the created payment method or an error response.</returns>
        [HttpPost("CreatePaymentMethod")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreatePaymentMethodResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest createRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.CreatePaymentMethodAsync(userResult.Value!.ID, createRequest);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Retrieves a specific payment method for the authenticated user.
        /// </summary>
        /// <param name="paymentMethodId">The ID of the payment method to retrieve.</param>
        /// <returns>Returns the payment method or an error response.</returns>
        [HttpGet("GetPaymentMethod/{paymentMethodId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentMethodResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentMethod(Guid paymentMethodId)
        {
            try
            {
                if (paymentMethodId == Guid.Empty)
                {
                    return BadRequest("Payment method ID is required.");
                }

                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.GetPaymentMethodAsync(userResult.Value!.ID, paymentMethodId);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 404, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Retrieves all payment methods for the authenticated user.
        /// </summary>
        /// <returns>Returns the list of payment methods or an error response.</returns>
        [HttpGet("GetUserPaymentMethods")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetPaymentMethodResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserPaymentMethods()
        {
            try
            {
                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.GetUserPaymentMethodsAsync(userResult.Value!.ID);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Retrieves all active payment methods for the authenticated user.
        /// </summary>
        /// <returns>Returns the list of active payment methods or an error response.</returns>
        [HttpGet("GetActiveUserPaymentMethods")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetPaymentMethodResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveUserPaymentMethods()
        {
            try
            {
                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.GetActiveUserPaymentMethodsAsync(userResult.Value!.ID);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Updates a payment method for the authenticated user.
        /// </summary>
        /// <param name="updateRequest">The payment method details to update.</param>
        /// <returns>Returns the updated payment method or an error response.</returns>
        [HttpPut("UpdatePaymentMethod")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdatePaymentMethodResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePaymentMethod([FromBody] UpdatePaymentMethodRequest updateRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.UpdatePaymentMethodAsync(userResult.Value!.ID, updateRequest);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Deletes a payment method for the authenticated user.
        /// </summary>
        /// <param name="paymentMethodId">The ID of the payment method to delete.</param>
        /// <returns>Returns a success message or an error response.</returns>
        [HttpDelete("DeletePaymentMethod/{paymentMethodId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeletePaymentMethodResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePaymentMethod(Guid paymentMethodId)
        {
            try
            {
                if (paymentMethodId == Guid.Empty)
                {
                    return BadRequest("Payment method ID is required.");
                }

                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.DeletePaymentMethodAsync(userResult.Value!.ID, paymentMethodId);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 404, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Sets a payment method as the default for the authenticated user.
        /// </summary>
        /// <param name="paymentMethodId">The ID of the payment method to set as default.</param>
        /// <returns>Returns the updated payment method or an error response.</returns>
        [HttpPost("SetDefaultPaymentMethod/{paymentMethodId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentMethodResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetDefaultPaymentMethod(Guid paymentMethodId)
        {
            try
            {
                if (paymentMethodId == Guid.Empty)
                {
                    return BadRequest("Payment method ID is required.");
                }

                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.SetDefaultPaymentMethodAsync(userResult.Value!.ID, paymentMethodId);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 400, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Retrieves the default payment method for the authenticated user.
        /// </summary>
        /// <returns>Returns the default payment method or an error response.</returns>
        [HttpGet("GetDefaultPaymentMethod")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentMethodResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDefaultPaymentMethod()
        {
            try
            {
                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure)
                {
                    return StatusCode(userResult.ErrorCode ?? 404, userResult.Error);
                }

                var result = await _paymentMethodService.GetDefaultPaymentMethodAsync(userResult.Value!.ID);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 404, result.Error);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}