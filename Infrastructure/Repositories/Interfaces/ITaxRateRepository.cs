using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ITaxRateRepository : IRepository<TaxRate>
    {
        Task<IEnumerable<TaxRate>> FindByCountryAsync(string country);
        Task<IEnumerable<TaxRate>> FindByProvinceStateAsync(string country, string provinceState);
        Task<IEnumerable<TaxRate>> FindActiveAsync();
        Task<IEnumerable<TaxRate>> FindByActiveStatusAsync(bool isActive);
        Task<bool> ExistsByNameAndLocationAsync(string nameEn, string country, string? provinceState);
    }
}