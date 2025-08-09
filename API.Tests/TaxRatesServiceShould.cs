using Domain.Services.Implementations;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class TaxRatesServiceShould
    {
        [Fact]
        public async Task GetTaxRateByIdAsync_ReturnSuccess_WhenTaxRateExists()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var service = new TaxRatesService(mockRepo.Object);
            var taxRateId = Guid.NewGuid();
            var taxRate = new TaxRate
            {
                ID = taxRateId,
                Name_en = "GST",
                Name_fr = "TPS",
                Country = "Canada",
                ProvinceState = null,
                Rate = 0.05m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            mockRepo.Setup(repo => repo.ExistsAsync(taxRateId))
                   .ReturnsAsync(true);
            mockRepo.Setup(repo => repo.GetByIdAsync(taxRateId))
                   .ReturnsAsync(taxRate);

            // Act
            var result = await service.GetTaxRateByIdAsync(taxRateId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(taxRateId, result.Value.ID);
            Assert.Equal("GST", result.Value.Name_en);
            Assert.Equal("TPS", result.Value.Name_fr);
            mockRepo.Verify(repo => repo.ExistsAsync(taxRateId), Times.Once);
            mockRepo.Verify(repo => repo.GetByIdAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task GetTaxRateByIdAsync_ReturnFailure_WhenTaxRateDoesNotExist()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var service = new TaxRatesService(mockRepo.Object);
            var taxRateId = Guid.NewGuid();

            mockRepo.Setup(repo => repo.ExistsAsync(taxRateId))
                   .ReturnsAsync(false);

            // Act
            var result = await service.GetTaxRateByIdAsync(taxRateId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Tax rate not found.", result.Error);
            Assert.Equal(StatusCodes.Status404NotFound, result.ErrorCode);
            mockRepo.Verify(repo => repo.ExistsAsync(taxRateId), Times.Once);
            mockRepo.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetTaxRateByIdAsync_ReturnFailure_WhenIdIsEmpty()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var service = new TaxRatesService(mockRepo.Object);

            // Act
            var result = await service.GetTaxRateByIdAsync(Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Tax rate ID is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            mockRepo.Verify(repo => repo.ExistsAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetAllTaxRatesAsync_ReturnSuccess_WhenCalled()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var service = new TaxRatesService(mockRepo.Object);
            var taxRates = new List<TaxRate>
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

            mockRepo.Setup(repo => repo.GetAllAsync())
                   .ReturnsAsync(taxRates);

            // Act
            var result = await service.GetAllTaxRatesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count());
            mockRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActiveTaxRatesAsync_ReturnSuccess_WhenCalled()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var service = new TaxRatesService(mockRepo.Object);
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
                }
            };

            mockRepo.Setup(repo => repo.FindActiveAsync())
                   .ReturnsAsync(activeTaxRates);

            // Act
            var result = await service.GetActiveTaxRatesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.All(result.Value, taxRate => Assert.True(taxRate.IsActive));
            mockRepo.Verify(repo => repo.FindActiveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTaxRatesByCountryAsync_ReturnSuccess_WhenCountryIsValid()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var service = new TaxRatesService(mockRepo.Object);
            var canadianTaxRates = new List<TaxRate>
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
                }
            };

            mockRepo.Setup(repo => repo.FindByCountryAsync("Canada"))
                   .ReturnsAsync(canadianTaxRates);

            // Act
            var result = await service.GetTaxRatesByCountryAsync("Canada");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.All(result.Value, taxRate => Assert.Equal("Canada", taxRate.Country));
            mockRepo.Verify(repo => repo.FindByCountryAsync("Canada"), Times.Once);
        }

        [Fact]
        public async Task GetTaxRatesByCountryAsync_ReturnFailure_WhenCountryIsEmpty()
        {
            // Arrange
            var mockRepo = new Mock<ITaxRateRepository>();
            var service = new TaxRatesService(mockRepo.Object);

            // Act
            var result = await service.GetTaxRatesByCountryAsync("");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Country is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            mockRepo.Verify(repo => repo.FindByCountryAsync(It.IsAny<string>()), Times.Never);
        }
    }
}