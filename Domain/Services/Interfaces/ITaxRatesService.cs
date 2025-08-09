using Domain.Models.Responses;
using Helpers.Common;

namespace Domain.Services.Interfaces
{
    public interface ITaxRatesService
    {
        Task<Result<GetTaxRateResponse>> GetTaxRateByIdAsync(Guid id);
        Task<Result<IEnumerable<GetTaxRateResponse>>> GetAllTaxRatesAsync();
        Task<Result<IEnumerable<GetTaxRateResponse>>> GetActiveTaxRatesAsync();
        Task<Result<IEnumerable<GetTaxRateResponse>>> GetTaxRatesByCountryAsync(string country);
        Task<Result<IEnumerable<GetTaxRateResponse>>> GetTaxRatesByLocationAsync(string country, string provinceState);
    }
}