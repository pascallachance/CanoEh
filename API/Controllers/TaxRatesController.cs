using System.Diagnostics;
using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxRatesController(ITaxRatesService taxRatesService) : ControllerBase
    {
        private readonly ITaxRatesService _taxRatesService = taxRatesService;

        /// <summary>
        /// Gets a tax rate by ID.
        /// </summary>
        /// <param name="id">The ID of the tax rate to retrieve.</param>
        /// <returns>Returns the tax rate or an error response.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTaxRateById(Guid id)
        {
            try
            {
                var result = await _taxRatesService.GetTaxRateByIdAsync(id);

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
        /// Gets all tax rates.
        /// </summary>
        /// <returns>Returns a list of all tax rates or an error response.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTaxRates()
        {
            try
            {
                var result = await _taxRatesService.GetAllTaxRatesAsync();

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
        /// Gets all active tax rates.
        /// </summary>
        /// <returns>Returns a list of active tax rates or an error response.</returns>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveTaxRates()
        {
            try
            {
                var result = await _taxRatesService.GetActiveTaxRatesAsync();

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
        /// Gets tax rates by country.
        /// </summary>
        /// <param name="country">The country to filter by.</param>
        /// <returns>Returns a list of tax rates for the specified country or an error response.</returns>
        [HttpGet("country/{country}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTaxRatesByCountry(string country)
        {
            try
            {
                var result = await _taxRatesService.GetTaxRatesByCountryAsync(country);

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
        /// Gets tax rates by country and province/state.
        /// </summary>
        /// <param name="country">The country to filter by.</param>
        /// <param name="provinceState">The province or state to filter by.</param>
        /// <returns>Returns a list of tax rates for the specified location or an error response.</returns>
        [HttpGet("location/{country}/{provinceState}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTaxRatesByLocation(string country, string provinceState)
        {
            try
            {
                var result = await _taxRatesService.GetTaxRatesByLocationAsync(country, provinceState);

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
        /// Updates a tax rate.
        /// </summary>
        /// <param name="id">The ID of the tax rate to update.</param>
        /// <param name="updateRequest">The updated tax rate details.</param>
        /// <returns>Returns the updated tax rate or an error response.</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTaxRate(Guid id, [FromBody] UpdateTaxRateRequest updateRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Ensure the ID in the URL matches the ID in the request body
                if (id != updateRequest.ID)
                {
                    return BadRequest("The ID in the URL does not match the ID in the request body.");
                }

                var result = await _taxRatesService.UpdateTaxRateAsync(updateRequest);

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
        /// Deactivates a tax rate (soft delete).
        /// </summary>
        /// <param name="id">The ID of the tax rate to deactivate.</param>
        /// <returns>Returns a success message or an error response.</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTaxRate(Guid id)
        {
            try
            {
                var result = await _taxRatesService.DeleteTaxRateAsync(id);

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