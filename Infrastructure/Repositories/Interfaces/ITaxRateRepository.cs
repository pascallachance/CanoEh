using Infrastructure.Data;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ITaxRateRepository
    {
        Task<TaxRate> GetByIdAsync(Guid id);
        Task<IEnumerable<TaxRate>> GetAllAsync();
        Task<IEnumerable<TaxRate>> FindAsync(Func<TaxRate, bool> predicate);
        Task<int> CountAsync(Func<TaxRate, bool> predicate);
        Task<bool> ExistsAsync(Guid id);
        Task<IEnumerable<TaxRate>> FindByCountryAsync(string country);
        Task<IEnumerable<TaxRate>> FindByProvinceStateAsync(string country, string provinceState);
        Task<IEnumerable<TaxRate>> FindActiveAsync();
        Task<IEnumerable<TaxRate>> FindByActiveStatusAsync(bool isActive);
        Task<bool> ExistsByNameAndLocationAsync(string nameEn, string country, string? provinceState);
    }
}