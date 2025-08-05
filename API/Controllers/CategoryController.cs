using System.Diagnostics;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        private readonly ICategoryService _categoryService = categoryService;

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="createCategoryRequest">The category details to create.</param>
        /// <returns>Returns the created category or an error response.</returns>
        [HttpPost("CreateCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest createCategoryRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _categoryService.CreateCategoryAsync(createCategoryRequest);

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
        /// Gets all categories.
        /// </summary>
        /// <returns>Returns all categories or an error response.</returns>
        [HttpGet("GetAllCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync();

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
        /// Gets a category by ID.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <returns>Returns the category or an error response.</returns>
        [HttpGet("GetCategoryById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);

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
        /// Gets all root categories (categories without a parent).
        /// </summary>
        /// <returns>Returns root categories or an error response.</returns>
        [HttpGet("GetRootCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRootCategories()
        {
            try
            {
                var result = await _categoryService.GetRootCategoriesAsync();

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
        /// Gets subcategories for a specific parent category.
        /// </summary>
        /// <param name="parentCategoryId">The parent category ID.</param>
        /// <returns>Returns subcategories or an error response.</returns>
        [HttpGet("GetSubcategories/{parentCategoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSubcategories(Guid parentCategoryId)
        {
            try
            {
                var result = await _categoryService.GetSubcategoriesAsync(parentCategoryId);

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
        /// Updates an existing category.
        /// </summary>
        /// <param name="updateCategoryRequest">The category details to update.</param>
        /// <returns>Returns the updated category or an error response.</returns>
        [HttpPut("UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest updateCategoryRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _categoryService.UpdateCategoryAsync(updateCategoryRequest);

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
        /// Deletes a category by ID.
        /// </summary>
        /// <param name="id">The category ID to delete.</param>
        /// <returns>Returns the deletion result or an error response.</returns>
        [HttpDelete("DeleteCategory/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);

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
    }
}