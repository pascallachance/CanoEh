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
    public class OrderController(IOrderService orderService, IUserService userService) : ControllerBase
    {
        private readonly IOrderService _orderService = orderService;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Creates a new order for the authenticated user.
        /// </summary>
        /// <param name="createRequest">The order details to create.</param>
        /// <returns>Returns the created order or an error response.</returns>
        [HttpPost("CreateOrder")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest createRequest)
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

                var user = userResult.Value!;
                var result = await _orderService.CreateOrderAsync(user.ID, createRequest);

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
        /// Gets an order by ID for the authenticated user.
        /// </summary>
        /// <param name="orderId">The order ID to retrieve.</param>
        /// <returns>Returns the order or an error response.</returns>
        [HttpGet("GetOrder/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetOrderResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrder(Guid orderId)
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

                var user = userResult.Value!;
                var result = await _orderService.GetOrderAsync(user.ID, orderId);

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
        /// Gets an order by order number for the authenticated user.
        /// </summary>
        /// <param name="orderNumber">The order number to retrieve.</param>
        /// <returns>Returns the order or an error response.</returns>
        [HttpGet("GetOrderByNumber/{orderNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetOrderResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderByNumber(int orderNumber)
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

                var user = userResult.Value!;
                var result = await _orderService.GetOrderByOrderNumberAsync(user.ID, orderNumber);

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
        /// Gets all orders for the authenticated user.
        /// </summary>
        /// <returns>Returns a list of orders or an error response.</returns>
        [HttpGet("GetOrders")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetOrderResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrders()
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

                var user = userResult.Value!;
                var result = await _orderService.GetUserOrdersAsync(user.ID);

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
        /// Gets orders by status for the authenticated user.
        /// </summary>
        /// <param name="statusCode">The status code to filter by.</param>
        /// <returns>Returns a list of orders with the specified status or an error response.</returns>
        [HttpGet("GetOrdersByStatus/{statusCode}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetOrderResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrdersByStatus(string statusCode)
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

                var user = userResult.Value!;
                var result = await _orderService.GetUserOrdersByStatusAsync(user.ID, statusCode);

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
        /// Updates an order for the authenticated user.
        /// </summary>
        /// <param name="updateRequest">The order details to update.</param>
        /// <returns>Returns the updated order or an error response.</returns>
        [HttpPut("UpdateOrder")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequest updateRequest)
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

                var user = userResult.Value!;
                var result = await _orderService.UpdateOrderAsync(user.ID, updateRequest);

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
        /// Updates an order status for the authenticated user.
        /// </summary>
        /// <param name="orderId">The order ID to update.</param>
        /// <param name="statusCode">The new status code.</param>
        /// <returns>Returns the updated order or an error response.</returns>
        [HttpPut("UpdateOrderStatus/{orderId}/{statusCode}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, string statusCode)
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

                var user = userResult.Value!;
                var result = await _orderService.UpdateOrderStatusAsync(user.ID, orderId, statusCode);

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
        /// Updates an order item status for the authenticated user.
        /// </summary>
        /// <param name="orderId">The order ID.</param>
        /// <param name="orderItemId">The order item ID to update.</param>
        /// <param name="status">The new status.</param>
        /// <param name="onHoldReason">Optional reason if status is OnHold.</param>
        /// <returns>Returns the updated order or an error response.</returns>
        [HttpPut("UpdateOrderItemStatus/{orderId}/{orderItemId}/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateOrderResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderItemStatus(Guid orderId, Guid orderItemId, string status, [FromQuery] string? onHoldReason = null)
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

                var user = userResult.Value!;
                var result = await _orderService.UpdateOrderItemStatusAsync(user.ID, orderId, orderItemId, status, onHoldReason);

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
        /// Deletes an order for the authenticated user.
        /// </summary>
        /// <param name="orderId">The order ID to delete.</param>
        /// <returns>Returns a deletion confirmation or an error response.</returns>
        [HttpDelete("DeleteOrder/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteOrderResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOrder(Guid orderId)
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

                var user = userResult.Value!;
                var result = await _orderService.DeleteOrderAsync(user.ID, orderId);

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