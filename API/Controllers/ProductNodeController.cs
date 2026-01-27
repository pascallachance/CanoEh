using System.Diagnostics;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductNodeController(IProductNodeService productNodeService) : ControllerBase
    {
        private readonly IProductNodeService _productNodeService = productNodeService;

        /// <summary>
        /// Creates a new product node (Departement, Navigation, or Category).
        /// </summary>
        /// <param name="request">The product node details to create.</param>
        /// <returns>Returns the created product node or an error response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateProductNode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProductNode([FromBody] CreateProductNodeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _productNodeService.CreateProductNodeAsync(request);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Gets all product nodes.
        /// </summary>
        /// <returns>Returns all product nodes or an error response.</returns>
        [HttpGet("GetAllProductNodes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProductNodes()
        {
            try
            {
                var result = await _productNodeService.GetAllProductNodesAsync();

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Gets a product node by ID.
        /// </summary>
        /// <param name="id">The product node ID.</param>
        /// <returns>Returns the product node or an error response.</returns>
        [HttpGet("GetProductNodeById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductNodeById(Guid id)
        {
            try
            {
                var result = await _productNodeService.GetProductNodeByIdAsync(id);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Gets all root nodes (Departement nodes).
        /// </summary>
        /// <returns>Returns all root nodes or an error response.</returns>
        [HttpGet("GetRootNodes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRootNodes()
        {
            try
            {
                var result = await _productNodeService.GetRootNodesAsync();

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Gets child nodes of a parent node.
        /// </summary>
        /// <param name="parentId">The parent node ID.</param>
        /// <returns>Returns child nodes or an error response.</returns>
        [HttpGet("GetChildren/{parentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChildren(Guid parentId)
        {
            try
            {
                var result = await _productNodeService.GetChildrenAsync(parentId);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Gets nodes by type (Departement, Navigation, or Category).
        /// </summary>
        /// <param name="nodeType">The node type.</param>
        /// <returns>Returns nodes of the specified type or an error response.</returns>
        [HttpGet("GetNodesByType/{nodeType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNodesByType(string nodeType)
        {
            try
            {
                var result = await _productNodeService.GetNodesByTypeAsync(nodeType);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Gets all category nodes.
        /// </summary>
        /// <returns>Returns all category nodes or an error response.</returns>
        [HttpGet("GetCategoryNodes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryNodes()
        {
            try
            {
                var result = await _productNodeService.GetCategoryNodesAsync();

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Updates a product node.
        /// </summary>
        /// <param name="request">The product node details to update.</param>
        /// <returns>Returns the updated product node or an error response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateProductNode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProductNode([FromBody] UpdateProductNodeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _productNodeService.UpdateProductNodeAsync(request);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Deletes a product node.
        /// </summary>
        /// <param name="id">The product node ID to delete.</param>
        /// <returns>Returns success response or an error response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteProductNode/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductNode(Guid id)
        {
            try
            {
                var result = await _productNodeService.DeleteProductNodeAsync(id);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 501, result.Error);
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
