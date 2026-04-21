using System.Diagnostics;
using System.Security.Claims;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemReviewController(IItemReviewService itemReviewService, IUserService userService) : ControllerBase
    {
        private const int ReviewReminderDelayMonths = 3;
        private readonly IItemReviewService _itemReviewService = itemReviewService;
        private readonly IUserService _userService = userService;

        [HttpPost("CreateItemReview")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateItemReview([FromBody] CreateItemReviewRequest request)
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
                    return StatusCode(userResult.ErrorCode ?? StatusCodes.Status404NotFound, userResult.Error);
                }

                var result = await _itemReviewService.CreateItemReviewAsync(userResult.Value!.ID, request);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("GetItemReviewById/{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetItemReviewById(Guid id)
        {
            try
            {
                var result = await _itemReviewService.GetItemReviewByIdAsync(id);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("GetItemReviewsByItem/{itemId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetItemReviewsByItem(Guid itemId)
        {
            try
            {
                var result = await _itemReviewService.GetItemReviewsByItemIdAsync(itemId);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("GetItemRatingSummary/{itemId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetItemRatingSummary(Guid itemId)
        {
            try
            {
                var result = await _itemReviewService.GetItemRatingSummaryAsync(itemId);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPut("UpdateItemReview")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItemReview([FromBody] UpdateItemReviewRequest request)
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
                    return StatusCode(userResult.ErrorCode ?? StatusCodes.Status404NotFound, userResult.Error);
                }

                var result = await _itemReviewService.UpdateItemReviewAsync(userResult.Value!.ID, request);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpDelete("DeleteItemReview/{id:guid}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItemReview(Guid id)
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
                    return StatusCode(userResult.ErrorCode ?? StatusCodes.Status404NotFound, userResult.Error);
                }

                var result = await _itemReviewService.DeleteItemReviewAsync(userResult.Value!.ID, id);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet("GetPendingReviewReminderCandidates")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingReviewReminderCandidates()
        {
            try
            {
                var cutoffUtc = DateTime.UtcNow.AddMonths(-ReviewReminderDelayMonths);
                var result = await _itemReviewService.GetPendingReviewReminderCandidatesAsync(cutoffUtc);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? StatusCodes.Status500InternalServerError, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}
