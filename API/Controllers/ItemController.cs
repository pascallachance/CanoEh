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
    public class ItemController(IItemService itemService, IFileStorageService fileStorageService, ILogger<ItemController> logger) : ControllerBase
    {
        private readonly IItemService _itemService = itemService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ILogger<ItemController> _logger = logger;

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
                _logger.LogInformation("=== UploadImage API START ===");
                _logger.LogInformation("Request parameters - variantId: {VariantId}, imageType: {ImageType}, imageNumber: {ImageNumber}", 
                    variantId, imageType, imageNumber);
                _logger.LogInformation("File info - FileName: {FileName}, Length: {Length}, ContentType: {ContentType}", 
                    file?.FileName, file?.Length, file?.ContentType);
                
                // Validate imageType
                if (imageType != "thumbnail" && imageType != "image")
                {
                    _logger.LogWarning("Invalid imageType: {ImageType}", imageType);
                    return BadRequest("Invalid imageType. Must be 'thumbnail' or 'image'.");
                }

                // Validate imageNumber
                if (imageType == "image" && imageNumber < 1)
                {
                    _logger.LogWarning("Invalid imageNumber: {ImageNumber}", imageNumber);
                    return BadRequest("imageNumber must be greater than 0.");
                }

                if (variantId == Guid.Empty)
                {
                    _logger.LogWarning("Empty variantId provided");
                    return BadRequest("variantId is required.");
                }

                // Get user ID from claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "userId");
                _logger.LogInformation("User claims - Count: {ClaimCount}, Found userId claim: {HasUserId}", 
                    User.Claims.Count(), userIdClaim != null);
                
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    _logger.LogWarning("User ID not found in token or invalid. UserIdClaim: {UserIdClaim}", userIdClaim?.Value);
                    return StatusCode(StatusCodes.Status401Unauthorized, "User ID not found in token.");
                }
                
                _logger.LogInformation("Authenticated user ID: {UserId}", userId);

                // Get the item by variant ID and verify ownership efficiently
                _logger.LogInformation("Retrieving item by variant ID: {VariantId} for user: {UserId}", variantId, userId);
                var itemResult = await _itemService.GetItemByVariantIdAsync(variantId, userId);
                if (itemResult.IsFailure)
                {
                    _logger.LogWarning("Failed to retrieve item by variant. ErrorCode: {ErrorCode}, Error: {Error}", 
                        itemResult.ErrorCode, itemResult.Error);
                    if (itemResult.ErrorCode == StatusCodes.Status404NotFound)
                        return NotFound("Variant not found or you do not have permission to upload images for this variant.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving item by variant.");
                }

                var item = itemResult.Value;
                _logger.LogInformation("Item retrieved successfully - ItemId: {ItemId}, SellerID: {SellerId}", item.Id, item.SellerID);

                // Use SellerID as the companyID
                var companyId = item.SellerID;
                _logger.LogInformation("Using SellerID as companyId: {CompanyId}", companyId);
                
                // Build the file path according to the spec
                // Format: {companyID}/{ItemVariantID}/{ItemVariantID}_thumb.jpg or {ItemVariantID}_{imageNumber}.jpg
                var subPath = $"{companyId}/{variantId}";
                var fileName = imageType == "thumbnail" 
                    ? $"{variantId}_thumb" 
                    : $"{variantId}_{imageNumber}";
                
                _logger.LogInformation("Upload parameters - SubPath: {SubPath}, FileName: {FileName}", subPath, fileName);

                _logger.LogInformation("Calling FileStorageService.UploadFileAsync...");
                var result = await _fileStorageService.UploadFileAsync(file, fileName, subPath);

                if (result.IsFailure)
                {
                    _logger.LogError("FileStorageService returned failure - ErrorCode: {ErrorCode}, Error: {Error}", 
                        result.ErrorCode, result.Error);
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                _logger.LogInformation("=== UploadImage API SUCCESS === Image URL: {ImageUrl}", result.Value);
                return Ok(new { imageUrl = result.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== UploadImage API FAILED === Exception: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}