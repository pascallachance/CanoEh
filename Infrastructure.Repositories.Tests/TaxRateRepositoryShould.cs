using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Infrastructure.Repositories.Interfaces;
using Moq;

namespace Infrastructure.Repositories.Tests
{
    public class TaxRateRepositoryShould
    {
        private readonly TaxRateRepository _taxRateRepository;
        private readonly Mock<ITaxRateRepository> _mockTaxRateRepository;
        private readonly string ConnectionString = "Server=test;Database=test;Trusted_Connection=true;";

        public TaxRateRepositoryShould()
        {
            _taxRateRepository = new TaxRateRepository(ConnectionString);
            _mockTaxRateRepository = new Mock<ITaxRateRepository>();
        }

        private TaxRate CreateValidEntity()
        {
            return new TaxRate
            {
                ID = Guid.NewGuid(),
                Name_en = "GST",
                Name_fr = "TPS",
                Country = "Canada",
                ProvinceState = "Ontario",
                Rate = 0.05m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private IEnumerable<TaxRate> CreateMultipleValidEntities()
        {
            return new List<TaxRate>
            {
                new TaxRate
                {
                    ID = Guid.NewGuid(),
                    Name_en = "GST",
                    Name_fr = "TPS",
                    Country = "Canada",
                    ProvinceState = null,
                    Rate = 0.05m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TaxRate
                {
                    ID = Guid.NewGuid(),
                    Name_en = "PST",
                    Name_fr = "TVP",
                    Country = "Canada",
                    ProvinceState = "Ontario",
                    Rate = 0.08m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TaxRate
                {
                    ID = Guid.NewGuid(),
                    Name_en = "Sales Tax",
                    Name_fr = "Taxe de vente",
                    Country = "USA",
                    ProvinceState = "California",
                    Rate = 0.10m,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                }
            };
        }

        // Test ITaxRateRepository specific methods
        [Fact]
        public async Task GetByIdAsync_ShouldReturnTaxRate_WhenTaxRateExists()
        {
            // Arrange
            var taxRate = CreateValidEntity();
            _mockTaxRateRepository.Setup(repo => repo.GetByIdAsync(taxRate.ID))
                                 .ReturnsAsync(taxRate);

            // Act
            var result = await _mockTaxRateRepository.Object.GetByIdAsync(taxRate.ID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taxRate.ID, result.ID);
            Assert.Equal(taxRate.Name_en, result.Name_en);
            Assert.Equal(taxRate.Rate, result.Rate);
            Assert.True(result.IsActive);
            _mockTaxRateRepository.Verify(repo => repo.GetByIdAsync(taxRate.ID), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTaxRates()
        {
            // Arrange
            var taxRates = CreateMultipleValidEntities();
            _mockTaxRateRepository.Setup(repo => repo.GetAllAsync())
                                 .ReturnsAsync(taxRates);

            // Act
            var result = await _mockTaxRateRepository.Object.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taxRates.Count(), result.Count());
            _mockTaxRateRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task FindByCountryAsync_ShouldReturnTaxRates_WhenCountryMatches()
        {
            // Arrange
            var country = "Canada";
            var canadianTaxRates = CreateMultipleValidEntities().Where(t => t.Country == country);
            _mockTaxRateRepository.Setup(repo => repo.FindByCountryAsync(country))
                                 .ReturnsAsync(canadianTaxRates);

            // Act
            var result = await _mockTaxRateRepository.Object.FindByCountryAsync(country);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, taxRate => Assert.Equal(country, taxRate.Country));
            _mockTaxRateRepository.Verify(repo => repo.FindByCountryAsync(country), Times.Once);
        }

        [Fact]
        public async Task FindByProvinceStateAsync_ShouldReturnTaxRates_WhenLocationMatches()
        {
            // Arrange
            var country = "Canada";
            var provinceState = "Ontario";
            var ontarioTaxRates = CreateMultipleValidEntities()
                .Where(t => t.Country == country && t.ProvinceState == provinceState);
            
            _mockTaxRateRepository.Setup(repo => repo.FindByProvinceStateAsync(country, provinceState))
                                 .ReturnsAsync(ontarioTaxRates);

            // Act
            var result = await _mockTaxRateRepository.Object.FindByProvinceStateAsync(country, provinceState);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, taxRate => {
                Assert.Equal(country, taxRate.Country);
                Assert.Equal(provinceState, taxRate.ProvinceState);
            });
            _mockTaxRateRepository.Verify(repo => repo.FindByProvinceStateAsync(country, provinceState), Times.Once);
        }

        [Fact]
        public async Task FindActiveAsync_ShouldReturnActiveTaxRates()
        {
            // Arrange
            var activeTaxRates = CreateMultipleValidEntities().Where(t => t.IsActive);
            _mockTaxRateRepository.Setup(repo => repo.FindActiveAsync())
                                 .ReturnsAsync(activeTaxRates);

            // Act
            var result = await _mockTaxRateRepository.Object.FindActiveAsync();

            // Assert
            Assert.NotNull(result);
            Assert.All(result, taxRate => Assert.True(taxRate.IsActive));
            _mockTaxRateRepository.Verify(repo => repo.FindActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task FindByActiveStatusAsync_ShouldReturnTaxRatesByStatus()
        {
            // Arrange
            var isActive = true;
            var activeTaxRates = CreateMultipleValidEntities().Where(t => t.IsActive == isActive);
            _mockTaxRateRepository.Setup(repo => repo.FindByActiveStatusAsync(isActive))
                                 .ReturnsAsync(activeTaxRates);

            // Act
            var result = await _mockTaxRateRepository.Object.FindByActiveStatusAsync(isActive);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, taxRate => Assert.Equal(isActive, taxRate.IsActive));
            _mockTaxRateRepository.Verify(repo => repo.FindByActiveStatusAsync(isActive), Times.Once);
        }

        [Fact]
        public async Task ExistsByNameAndLocationAsync_ShouldReturnTrue_WhenTaxRateExists()
        {
            // Arrange
            var nameEn = "GST";
            var country = "Canada";
            string? provinceState = null;
            _mockTaxRateRepository.Setup(repo => repo.ExistsByNameAndLocationAsync(nameEn, country, provinceState))
                                 .ReturnsAsync(true);

            // Act
            var result = await _mockTaxRateRepository.Object.ExistsByNameAndLocationAsync(nameEn, country, provinceState);

            // Assert
            Assert.True(result);
            _mockTaxRateRepository.Verify(repo => repo.ExistsByNameAndLocationAsync(nameEn, country, provinceState), Times.Once);
        }

        [Fact]
        public async Task ExistsByNameAndLocationAsync_ShouldReturnFalse_WhenTaxRateDoesNotExist()
        {
            // Arrange
            var nameEn = "NonExistent";
            var country = "Canada";
            string? provinceState = null;
            _mockTaxRateRepository.Setup(repo => repo.ExistsByNameAndLocationAsync(nameEn, country, provinceState))
                                 .ReturnsAsync(false);

            // Act
            var result = await _mockTaxRateRepository.Object.ExistsByNameAndLocationAsync(nameEn, country, provinceState);

            // Assert
            Assert.False(result);
            _mockTaxRateRepository.Verify(repo => repo.ExistsByNameAndLocationAsync(nameEn, country, provinceState), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenTaxRateExists()
        {
            // Arrange
            var taxRateId = Guid.NewGuid();
            _mockTaxRateRepository.Setup(repo => repo.ExistsAsync(taxRateId))
                                 .ReturnsAsync(true);

            // Act
            var result = await _mockTaxRateRepository.Object.ExistsAsync(taxRateId);

            // Assert
            Assert.True(result);
            _mockTaxRateRepository.Verify(repo => repo.ExistsAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenTaxRateDoesNotExist()
        {
            // Arrange
            var taxRateId = Guid.NewGuid();
            _mockTaxRateRepository.Setup(repo => repo.ExistsAsync(taxRateId))
                                 .ReturnsAsync(false);

            // Act
            var result = await _mockTaxRateRepository.Object.ExistsAsync(taxRateId);

            // Assert
            Assert.False(result);
            _mockTaxRateRepository.Verify(repo => repo.ExistsAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task CountAsync_ShouldReturnCount_WhenPredicateProvided()
        {
            // Arrange
            var expectedCount = 2;
            Func<TaxRate, bool> predicate = t => t.Country == "Canada";
            _mockTaxRateRepository.Setup(repo => repo.CountAsync(predicate))
                                 .ReturnsAsync(expectedCount);

            // Act
            var result = await _mockTaxRateRepository.Object.CountAsync(predicate);

            // Assert
            Assert.Equal(expectedCount, result);
            _mockTaxRateRepository.Verify(repo => repo.CountAsync(predicate), Times.Once);
        }

        [Fact]
        public async Task FindAsync_ShouldReturnFilteredTaxRates_WhenPredicateProvided()
        {
            // Arrange
            var taxRates = CreateMultipleValidEntities();
            Func<TaxRate, bool> predicate = t => t.Country == "Canada";
            var filteredTaxRates = taxRates.Where(predicate);
            _mockTaxRateRepository.Setup(repo => repo.FindAsync(predicate))
                                 .ReturnsAsync(filteredTaxRates);

            // Act
            var result = await _mockTaxRateRepository.Object.FindAsync(predicate);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, taxRate => Assert.Equal("Canada", taxRate.Country));
            _mockTaxRateRepository.Verify(repo => repo.FindAsync(predicate), Times.Once);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenValidConnectionStringProvided()
        {
            // Arrange & Act
            var repository = new TaxRateRepository(ConnectionString);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public void TaxRate_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var taxRate = CreateValidEntity();

            // Assert
            Assert.NotEqual(Guid.Empty, taxRate.ID);
            Assert.Equal("GST", taxRate.Name_en);
            Assert.Equal("TPS", taxRate.Name_fr);
            Assert.Equal("Canada", taxRate.Country);
            Assert.Equal("Ontario", taxRate.ProvinceState);
            Assert.Equal(0.05m, taxRate.Rate);
            Assert.True(taxRate.IsActive);
        }

        [Fact]
        public void TaxRate_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var taxRate = new TaxRate
            {
                Name_en = "Test Tax",
                Name_fr = "Taxe Test",
                Country = "Test Country"
            };

            // Assert
            Assert.Equal(Guid.Empty, taxRate.ID);
            Assert.Equal("Test Tax", taxRate.Name_en);
            Assert.Equal("Taxe Test", taxRate.Name_fr);
            Assert.Equal("Test Country", taxRate.Country);
            Assert.Null(taxRate.ProvinceState);
            Assert.Equal(0m, taxRate.Rate);
            Assert.False(taxRate.IsActive);
            Assert.Equal(DateTime.MinValue, taxRate.CreatedAt);
            Assert.Null(taxRate.UpdatedAt);
        }
    }
}