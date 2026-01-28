using System.Diagnostics;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryNodeController(ICategoryNodeService categoryNodeService) : ControllerBase
    {
        private readonly ICategoryNodeService _categoryNodeService = categoryNodeService;

        /// <summary>
        /// Creates a new category node (Departement, Navigation, or Category).
        /// When creating a Category node, you can optionally include CategoryMandatoryAttributes
        /// which will be created in the same operation and linked to the new Category node.
        /// </summary>
        /// <param name="request">The category node details to create, optionally including CategoryMandatoryAttributes for Category nodes.</param>
        /// <returns>Returns the created category node with any created mandatory attributes or an error response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateCategoryNode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCategoryNode([FromBody] CreateCategoryNodeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _categoryNodeService.CreateCategoryNodeAsync(request);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
        /// Creates a complete hierarchical structure of category nodes.
        /// Allows bulk creation of multiple Departement nodes, each containing multiple Navigation nodes,
        /// which in turn can contain other Navigation nodes or Category nodes.
        /// All nodes are created in a single transaction - if any node fails validation or creation,
        /// the entire operation is rolled back.
        /// </summary>
        /// <param name="request">The hierarchical structure to create, containing one or more Departement nodes.</param>
        /// <returns>Returns the created structure with generated IDs or an error response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("CreateStructure")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateStructure([FromBody] BulkCreateStructureRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _categoryNodeService.CreateStructureAsync(request);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
        [HttpGet("GetAllCategoryNodes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategoryNodes()
        {
            try
            {
                var result = await _categoryNodeService.GetAllCategoryNodesAsync();

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
        /// Gets a category node by ID.
        /// </summary>
        /// <param name="id">The category node ID.</param>
        /// <returns>Returns the category node or an error response.</returns>
        [HttpGet("GetCategoryNodeById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryNodeById(Guid id)
        {
            try
            {
                var result = await _categoryNodeService.GetCategoryNodeByIdAsync(id);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
                var result = await _categoryNodeService.GetRootNodesAsync();

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
                var result = await _categoryNodeService.GetChildrenAsync(parentId);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
                var result = await _categoryNodeService.GetNodesByTypeAsync(nodeType);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
                var result = await _categoryNodeService.GetCategoryNodesAsync();

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
        /// Updates a category node.
        /// </summary>
        /// <param name="request">The category node details to update.</param>
        /// <returns>Returns the updated category node or an error response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateCategoryNode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategoryNode([FromBody] UpdateCategoryNodeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _categoryNodeService.UpdateCategoryNodeAsync(request);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
        /// Deletes a category node.
        /// </summary>
        /// <param name="id">The category node ID to delete.</param>
        /// <returns>Returns success response or an error response.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteCategoryNode/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategoryNode(Guid id)
        {
            try
            {
                var result = await _categoryNodeService.DeleteCategoryNodeAsync(id);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
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
