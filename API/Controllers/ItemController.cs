using System.Diagnostics;
using System.Security.Claims;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController(IItemService itemService, IFileStorageService fileStorageService) : ControllerBase
    {
        private readonly IItemService _itemService = itemService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="createItemRequest">The item details to create.</param>
        /// <returns>Returns the created item or an error response.</returns>
        [HttpPost("CreateItem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateItem([FromBody] CreateItemRequest createItemRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _itemService.CreateItemAsync(createItemRequest);

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
        /// Gets all items.
        /// </summary>
        /// <returns>Returns a list of all items or an error response.</returns>
        [HttpGet("GetAllItems")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllItems()
        {
            try
            {
                var result = await _itemService.GetAllItemsAsync();

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
        /// Gets an item by ID.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <returns>Returns the item or an error response.</returns>
        [HttpGet("GetItemById/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetItemById(Guid id)
        {
            try
            {
                var result = await _itemService.GetItemByIdAsync(id);

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
        /// Gets all items from a seller by seller ID.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>Returns a list of items from the seller or an error response.</returns>
        [HttpGet("GetSellerItems/{sellerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSellerItems(Guid sellerId)
        {
            try
            {
                var result = await _itemService.GetAllItemsFromSellerAsync(sellerId);

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
        /// Updates an existing item.
        /// </summary>
        /// <param name="updateItemRequest">The item details to update.</param>
        /// <returns>Returns the updated item or an error response.</returns>
        [HttpPut("UpdateItem")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateItemRequest updateItemRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _itemService.UpdateItemAsync(updateItemRequest);

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
        /// Soft deletes an item.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <returns>Returns a success response or an error response.</returns>
        [HttpDelete("DeleteItem/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            try
            {
                var result = await _itemService.DeleteItemAsync(id);

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
        /// Soft deletes an item variant.
        /// </summary>
        /// <param name="itemId">The ID of the item.</param>
        /// <param name="variantId">The ID of the variant to delete.</param>
        /// <returns>Returns a success response or an error response.</returns>
        [HttpDelete("DeleteItemVariant/{itemId:guid}/{variantId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItemVariant(Guid itemId, Guid variantId)
        {
            try
            {
                var result = await _itemService.DeleteItemVariantAsync(itemId, variantId);

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
        /// Uploads a product image.
        /// </summary>
        /// <param name="file">The image file to upload.</param>
        /// <param name="itemId">Optional item ID to associate the image with.</param>
        /// <returns>Returns the image URL or an error response.</returns>
        [HttpPost("UploadImage")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] Guid? itemId = null)
        {
            try
            {
                // If itemId is provided, validate that the item exists and is owned by the authenticated user
                string? fileName = null;
                if (itemId.HasValue)
                {
                    // Get user ID from claims
                    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "userId");
                    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                    {
                        return StatusCode(StatusCodes.Status401Unauthorized, "User ID not found in token.");
                    }

                    var itemResult = await _itemService.GetItemByIdAsync(itemId.Value);
                    if (itemResult.IsFailure)
                    {
                        return NotFound("Item not found.");
                    }

                    if (itemResult.Value?.SellerID != userId)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to upload images for this item.");
                    }

                    fileName = $"item_{itemId.Value}";
                }

                var result = await _fileStorageService.UploadFileAsync(file, fileName);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(new { imageUrl = result.Value });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}