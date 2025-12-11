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
        /// Uploads a product image (thumbnail or additional images).
        /// </summary>
        /// <param name="file">The image file to upload.</param>
        /// <param name="variantId">The variant ID to associate the image with.</param>
        /// <param name="imageType">Type of image: "thumbnail" or "image".</param>
        /// <param name="imageNumber">Image number for additional images (e.g., 1, 2, 3). Only used when imageType is "image".</param>
        /// <returns>Returns the image URL or an error response.</returns>
        [HttpPost("UploadImage")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadImage(
            IFormFile file, 
            [FromQuery] Guid variantId,
            [FromQuery] string imageType = "image",
            [FromQuery] int imageNumber = 1)
        {
            try
            {
                // Validate imageType
                if (imageType != "thumbnail" && imageType != "image")
                {
                    return BadRequest("Invalid imageType. Must be 'thumbnail' or 'image'.");
                }

                // Validate imageNumber
                if (imageType == "image" && imageNumber < 1)
                {
                    return BadRequest("imageNumber must be greater than 0.");
                }

                if (variantId == Guid.Empty)
                {
                    return BadRequest("variantId is required.");
                }

                // Get user ID from claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "userId");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, "User ID not found in token.");
                }

                // Get the item by variant ID and verify ownership
                // Note: This loads all items from the seller to find the variant. This is acceptable
                // because it's filtered by userId (only user's items), and we need to verify ownership.
                // Future optimization: Add GetItemByVariantIdAsync(variantId, userId) to ItemService
                // to avoid loading unnecessary data when dealing with sellers that have many items.
                var allItemsResult = await _itemService.GetAllItemsFromSellerAsync(userId);
                if (allItemsResult.IsFailure)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving items.");
                }

                var item = allItemsResult.Value?.FirstOrDefault(i => 
                    i.Variants?.Any(v => v.Id == variantId) == true);
                
                if (item == null)
                {
                    return NotFound("Variant not found or you do not have permission to upload images for this variant.");
                }

                // Use SellerID as the companyID
                var companyId = item.SellerID;
                
                // Build the file path according to the spec
                // Format: {companyID}/{ItemVariantID}/{ItemVariantID}_thumb.jpg or {ItemVariantID}_{imageNumber}.jpg
                var subPath = $"{companyId}/{variantId}";
                var fileName = imageType == "thumbnail" 
                    ? $"{variantId}_thumb" 
                    : $"{variantId}_{imageNumber}";

                var result = await _fileStorageService.UploadFileAsync(file, fileName, subPath);

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