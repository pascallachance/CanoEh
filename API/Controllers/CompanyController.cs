using System.Diagnostics;
using System.Security.Claims;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController(ICompanyService companyService, IUserService userService, IFileStorageService fileStorageService) : ControllerBase
    {
        private readonly ICompanyService _companyService = companyService;
        private readonly IUserService _userService = userService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        /// <summary>
        /// Creates a new company.
        /// The user must be authenticated.
        /// </summary>
        /// <param name="newCompany">The company details to create.</param>
        /// <returns>Returns the created company or an error response.</returns>
        [Authorize]
        [HttpPost("CreateCompany")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateCompanyResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest newCompany)
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

                // Get the authenticated user to verify they exist and get their ID
                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return Unauthorized("Invalid user.");
                }

                var result = await _companyService.CreateCompanyAsync(newCompany, userResult.Value.ID);

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
        /// Retrieves the details of a company by its ID.
        /// </summary>
        [HttpGet("GetCompany/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetCompanyResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompany(Guid companyId)
        {
            try
            {
                if (companyId == Guid.Empty)
                {
                    return BadRequest("Company ID is required.");
                }

                var result = await _companyService.GetCompanyAsync(companyId);
                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 404, result.Error);
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
        /// Retrieves all companies owned by the authenticated user.
        /// The user must be authenticated.
        /// </summary>
        [Authorize]
        [HttpGet("GetMyCompanies")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetCompanyResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyCompanies()
        {
            try
            {
                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                // Get the authenticated user to get their ID
                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return Unauthorized("Invalid user.");
                }

                var result = await _companyService.GetCompaniesByOwnerAsync(userResult.Value.ID);
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
        /// Updates the details of a company.
        /// The user must be authenticated and must be the owner of the company.
        /// </summary>
        [Authorize]
        [HttpPut("UpdateCompany")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateCompanyResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyRequest updateRequest)
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

                // Get the authenticated user to verify they exist and get their ID
                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return Unauthorized("Invalid user.");
                }

                // Ensure the OwnerID in the request matches the authenticated user
                if (updateRequest.OwnerID != userResult.Value.ID)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own companies.");
                }

                var result = await _companyService.UpdateCompanyAsync(updateRequest);
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
        /// Deletes a company by its ID.
        /// The user must be authenticated and must be the owner of the company.
        /// </summary>
        [Authorize]
        [HttpDelete("DeleteCompany/{companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteCompanyResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            try
            {
                if (companyId == Guid.Empty)
                {
                    return BadRequest("Company ID is required.");
                }

                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                // Get the authenticated user to verify they exist and get their ID
                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return Unauthorized("Invalid user.");
                }

                var result = await _companyService.DeleteCompanyAsync(companyId, userResult.Value.ID);
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
        /// Uploads a company logo.
        /// The user must be authenticated and must be the owner of the company.
        /// </summary>
        /// <param name="file">The logo image file to upload.</param>
        /// <param name="companyId">The company ID to associate the logo with.</param>
        /// <returns>Returns the logo URL or an error response.</returns>
        [Authorize]
        [HttpPost("UploadLogo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadLogo(IFormFile file, [FromQuery] Guid companyId)
        {
            try
            {
                if (companyId == Guid.Empty)
                {
                    return BadRequest("Company ID is required.");
                }

                var authenticatedEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedEmail))
                {
                    return Unauthorized("User not authenticated.");
                }

                // Get the authenticated user to verify they exist and get their ID
                var userResult = await _userService.GetUserEntityAsync(authenticatedEmail);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return Unauthorized("Invalid user.");
                }

                // Get the company and verify ownership
                var companyResult = await _companyService.GetCompanyAsync(companyId);
                if (companyResult.IsFailure)
                {
                    return NotFound("Company not found.");
                }

                if (companyResult.Value?.OwnerID != userResult.Value.ID)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to upload a logo for this company.");
                }

                // Build the file path according to the spec
                // Format: {companyID}/{companyID}_logo.jpg
                var fileName = $"{companyId}_logo";
                var subPath = companyId.ToString();

                var result = await _fileStorageService.UploadFileAsync(file, fileName, subPath);

                if (result.IsFailure)
                {
                    return StatusCode(result.ErrorCode ?? 500, result.Error);
                }

                return Ok(new { logoUrl = result.Value });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}