using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Moq;

namespace API.Tests
{
    public class TaxRateRepositoryShould
    {
        [Fact]
        public async Task GetByIdAsync_ReturnTaxRate_WhenTaxRateExists()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var testTaxRate = new TaxRate
            {
                ID = Guid.NewGuid(),
                Name_en = "GST",
                Name_fr = "TPS",
                Country = "Canada",
                ProvinceState = null,
                Rate = 0.05m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockRepo.Setup(repo => repo.GetByIdAsync(testTaxRate.ID))
                   .ReturnsAsync(testTaxRate);

            // Act
            var result = await mockRepo.Object.GetByIdAsync(testTaxRate.ID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testTaxRate.ID, result.ID);
            Assert.Equal("GST", result.Name_en);
            Assert.Equal("TPS", result.Name_fr);
            Assert.Equal("Canada", result.Country);
            Assert.Equal(0.05m, result.Rate);
            Assert.True(result.IsActive);
            mockRepo.Verify(repo => repo.GetByIdAsync(testTaxRate.ID), Times.Once);
        }

        [Fact]
        public async Task FindByCountryAsync_ReturnTaxRates_WhenCountryMatches()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var canadianTaxRates = new List<TaxRate>
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
                }
            };

            mockRepo.Setup(repo => repo.FindByCountryAsync("Canada"))
                   .ReturnsAsync(canadianTaxRates);

            // Act
            var result = await mockRepo.Object.FindByCountryAsync("Canada");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, taxRate => Assert.Equal("Canada", taxRate.Country));
            mockRepo.Verify(repo => repo.FindByCountryAsync("Canada"), Times.Once);
        }

        [Fact]
        public async Task FindActiveAsync_ReturnActiveTaxRates_WhenCalled()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var activeTaxRates = new List<TaxRate>
            {
                new TaxRate
                {
                    ID = Guid.NewGuid(),
                    Name_en = "GST",
                    Name_fr = "TPS",
                    Country = "Canada",
                    Rate = 0.05m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TaxRate
                {
                    ID = Guid.NewGuid(),
                    Name_en = "VAT",
                    Name_fr = "TVA",
                    Country = "France",
                    Rate = 0.20m,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            mockRepo.Setup(repo => repo.FindActiveAsync())
                   .ReturnsAsync(activeTaxRates);

            // Act
            var result = await mockRepo.Object.FindActiveAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, taxRate => Assert.True(taxRate.IsActive));
            mockRepo.Verify(repo => repo.FindActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ReturnTrue_WhenTaxRateExists()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var taxRateId = Guid.NewGuid();

            mockRepo.Setup(repo => repo.ExistsAsync(taxRateId))
                   .ReturnsAsync(true);

            // Act
            var result = await mockRepo.Object.ExistsAsync(taxRateId);

            // Assert
            Assert.True(result);
            mockRepo.Verify(repo => repo.ExistsAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_ReturnFalse_WhenTaxRateDoesNotExist()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var taxRateId = Guid.NewGuid();

            mockRepo.Setup(repo => repo.ExistsAsync(taxRateId))
                   .ReturnsAsync(false);

            // Act
            var result = await mockRepo.Object.ExistsAsync(taxRateId);

            // Assert
            Assert.False(result);
            mockRepo.Verify(repo => repo.ExistsAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task FindByProvinceStateAsync_ReturnTaxRates_WhenLocationMatches()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var ontarioTaxRates = new List<TaxRate>
            {
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
                }
            };

            mockRepo.Setup(repo => repo.FindByProvinceStateAsync("Canada", "Ontario"))
                   .ReturnsAsync(ontarioTaxRates);

            // Act
            var result = await mockRepo.Object.FindByProvinceStateAsync("Canada", "Ontario");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Ontario", result.First().ProvinceState);
            Assert.Equal("Canada", result.First().Country);
            mockRepo.Verify(repo => repo.FindByProvinceStateAsync("Canada", "Ontario"), Times.Once);
        }

        [Fact]
        public async Task ExistsByNameAndLocationAsync_ReturnTrue_WhenTaxRateExists()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();

            mockRepo.Setup(repo => repo.ExistsByNameAndLocationAsync("GST", "Canada", null))
                   .ReturnsAsync(true);

            // Act
            var result = await mockRepo.Object.ExistsByNameAndLocationAsync("GST", "Canada", null);

            // Assert
            Assert.True(result);
            mockRepo.Verify(repo => repo.ExistsByNameAndLocationAsync("GST", "Canada", null), Times.Once);
        }
    }
}