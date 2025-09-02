using System.Security.Claims;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Seller.Server.Controllers
{
    [Route("api/seller/[controller]")]
    [ApiController]
    public class CompanyController(ICompanyService companyService, IUserService userService) : ControllerBase
    {
        private readonly ICompanyService _companyService = companyService;
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Retrieves all companies owned by the authenticated user.
        /// The user must be authenticated.
        /// </summary>
        [Authorize]
        [HttpGet("my-companies")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new company.
        /// The user must be authenticated.
        /// </summary>
        /// <param name="newCompany">The company details to create.</param>
        /// <returns>Returns the created company or an error response.</returns>
        [Authorize]
        [HttpPost("create")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}