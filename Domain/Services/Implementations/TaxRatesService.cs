using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Domain.Services.Implementations
{
    public class TaxRatesService(ITaxRateRepository taxRateRepository) : ITaxRatesService
    {
        private readonly ITaxRateRepository _taxRateRepository = taxRateRepository;

        public async Task<Result<GetTaxRateResponse>> GetTaxRateByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return Result.Failure<GetTaxRateResponse>("Tax rate ID is required.", StatusCodes.Status400BadRequest);
                }

                var taxRateExists = await _taxRateRepository.ExistsAsync(id);
                if (!taxRateExists)
                {
                    return Result.Failure<GetTaxRateResponse>("Tax rate not found.", StatusCodes.Status404NotFound);
                }

                var taxRate = await _taxRateRepository.GetByIdAsync(id);
                var response = new GetTaxRateResponse
                {
                    ID = taxRate.ID,
                    Name_en = taxRate.Name_en,
                    Name_fr = taxRate.Name_fr,
                    Country = taxRate.Country,
                    ProvinceState = taxRate.ProvinceState,
                    Rate = taxRate.Rate,
                    IsActive = taxRate.IsActive,
                    CreatedAt = taxRate.CreatedAt,
                    UpdatedAt = taxRate.UpdatedAt
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<GetTaxRateResponse>($"An error occurred while retrieving the tax rate: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetTaxRateResponse>>> GetAllTaxRatesAsync()
        {
            try
            {
                var taxRates = await _taxRateRepository.GetAllAsync();
                var response = taxRates.Select(taxRate => new GetTaxRateResponse
                {
                    ID = taxRate.ID,
                    Name_en = taxRate.Name_en,
                    Name_fr = taxRate.Name_fr,
                    Country = taxRate.Country,
                    ProvinceState = taxRate.ProvinceState,
                    Rate = taxRate.Rate,
                    IsActive = taxRate.IsActive,
                    CreatedAt = taxRate.CreatedAt,
                    UpdatedAt = taxRate.UpdatedAt
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetTaxRateResponse>>($"An error occurred while retrieving tax rates: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetTaxRateResponse>>> GetActiveTaxRatesAsync()
        {
            try
            {
                var taxRates = await _taxRateRepository.FindActiveAsync();
                var response = taxRates.Select(taxRate => new GetTaxRateResponse
                {
                    ID = taxRate.ID,
                    Name_en = taxRate.Name_en,
                    Name_fr = taxRate.Name_fr,
                    Country = taxRate.Country,
                    ProvinceState = taxRate.ProvinceState,
                    Rate = taxRate.Rate,
                    IsActive = taxRate.IsActive,
                    CreatedAt = taxRate.CreatedAt,
                    UpdatedAt = taxRate.UpdatedAt
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetTaxRateResponse>>($"An error occurred while retrieving active tax rates: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetTaxRateResponse>>> GetTaxRatesByCountryAsync(string country)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(country))
                {
                    return Result.Failure<IEnumerable<GetTaxRateResponse>>("Country is required.", StatusCodes.Status400BadRequest);
                }

                var taxRates = await _taxRateRepository.FindByCountryAsync(country);
                var response = taxRates.Select(taxRate => new GetTaxRateResponse
                {
                    ID = taxRate.ID,
                    Name_en = taxRate.Name_en,
                    Name_fr = taxRate.Name_fr,
                    Country = taxRate.Country,
                    ProvinceState = taxRate.ProvinceState,
                    Rate = taxRate.Rate,
                    IsActive = taxRate.IsActive,
                    CreatedAt = taxRate.CreatedAt,
                    UpdatedAt = taxRate.UpdatedAt
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetTaxRateResponse>>($"An error occurred while retrieving tax rates by country: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<Result<IEnumerable<GetTaxRateResponse>>> GetTaxRatesByLocationAsync(string country, string provinceState)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(country))
                {
                    return Result.Failure<IEnumerable<GetTaxRateResponse>>("Country is required.", StatusCodes.Status400BadRequest);
                }

                if (string.IsNullOrWhiteSpace(provinceState))
                {
                    return Result.Failure<IEnumerable<GetTaxRateResponse>>("Province/State is required.", StatusCodes.Status400BadRequest);
                }

                var taxRates = await _taxRateRepository.FindByProvinceStateAsync(country, provinceState);
                var response = taxRates.Select(taxRate => new GetTaxRateResponse
                {
                    ID = taxRate.ID,
                    Name_en = taxRate.Name_en,
                    Name_fr = taxRate.Name_fr,
                    Country = taxRate.Country,
                    ProvinceState = taxRate.ProvinceState,
                    Rate = taxRate.Rate,
                    IsActive = taxRate.IsActive,
                    CreatedAt = taxRate.CreatedAt,
                    UpdatedAt = taxRate.UpdatedAt
                });

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                return Result.Failure<IEnumerable<GetTaxRateResponse>>($"An error occurred while retrieving tax rates by location: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }
    }
}