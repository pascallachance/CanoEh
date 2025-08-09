using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests
{
    public class TaxRatesControllerShould
    {
        [Fact]
        public async Task GetTaxRateById_ReturnOk_WhenTaxRateExists()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var taxRateId = Guid.NewGuid();
            var taxRateResponse = new GetTaxRateResponse
            {
                ID = taxRateId,
                Name_en = "GST",
                Name_fr = "TPS",
                Country = "Canada",
                Rate = 0.05m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            mockService.Setup(service => service.GetTaxRateByIdAsync(taxRateId))
                      .ReturnsAsync(Result.Success(taxRateResponse));

            // Act
            var result = await controller.GetTaxRateById(taxRateId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            mockService.Verify(service => service.GetTaxRateByIdAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task GetTaxRateById_ReturnNotFound_WhenTaxRateDoesNotExist()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var taxRateId = Guid.NewGuid();

            mockService.Setup(service => service.GetTaxRateByIdAsync(taxRateId))
                      .ReturnsAsync(Result.Failure<GetTaxRateResponse>("Tax rate not found.", StatusCodes.Status404NotFound));

            // Act
            var result = await controller.GetTaxRateById(taxRateId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
            mockService.Verify(service => service.GetTaxRateByIdAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task GetAllTaxRates_ReturnOk_WhenCalled()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var taxRates = new List<GetTaxRateResponse>
            {
                new GetTaxRateResponse
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

            mockService.Setup(service => service.GetAllTaxRatesAsync())
                      .ReturnsAsync(Result.Success<IEnumerable<GetTaxRateResponse>>(taxRates));

            // Act
            var result = await controller.GetAllTaxRates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            mockService.Verify(service => service.GetAllTaxRatesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActiveTaxRates_ReturnOk_WhenCalled()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var activeTaxRates = new List<GetTaxRateResponse>
            {
                new GetTaxRateResponse
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

            mockService.Setup(service => service.GetActiveTaxRatesAsync())
                      .ReturnsAsync(Result.Success<IEnumerable<GetTaxRateResponse>>(activeTaxRates));

            // Act
            var result = await controller.GetActiveTaxRates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            mockService.Verify(service => service.GetActiveTaxRatesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTaxRatesByCountry_ReturnOk_WhenCountryIsValid()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var country = "Canada";
            var canadianTaxRates = new List<GetTaxRateResponse>
            {
                new GetTaxRateResponse
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

            mockService.Setup(service => service.GetTaxRatesByCountryAsync(country))
                      .ReturnsAsync(Result.Success<IEnumerable<GetTaxRateResponse>>(canadianTaxRates));

            // Act
            var result = await controller.GetTaxRatesByCountry(country);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            mockService.Verify(service => service.GetTaxRatesByCountryAsync(country), Times.Once);
        }

        [Fact]
        public async Task GetTaxRatesByLocation_ReturnOk_WhenLocationIsValid()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var country = "Canada";
            var provinceState = "Ontario";
            var locationTaxRates = new List<GetTaxRateResponse>
            {
                new GetTaxRateResponse
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

            mockService.Setup(service => service.GetTaxRatesByLocationAsync(country, provinceState))
                      .ReturnsAsync(Result.Success<IEnumerable<GetTaxRateResponse>>(locationTaxRates));

            // Act
            var result = await controller.GetTaxRatesByLocation(country, provinceState);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            mockService.Verify(service => service.GetTaxRatesByLocationAsync(country, provinceState), Times.Once);
        }

        [Fact]
        public async Task UpdateTaxRate_ReturnOk_WhenRequestIsValid()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var taxRateId = Guid.NewGuid();
            var updateRequest = new UpdateTaxRateRequest
            {
                ID = taxRateId,
                Name_en = "Updated GST",
                Name_fr = "TPS Mise à jour",
                Country = "Canada",
                Rate = 0.06m,
                IsActive = true
            };
            var updateResponse = new UpdateTaxRateResponse
            {
                ID = taxRateId,
                Name_en = "Updated GST",
                Name_fr = "TPS Mise à jour",
                Country = "Canada",
                Rate = 0.06m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            mockService.Setup(service => service.UpdateTaxRateAsync(updateRequest))
                      .ReturnsAsync(Result.Success(updateResponse));

            // Act
            var result = await controller.UpdateTaxRate(taxRateId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            mockService.Verify(service => service.UpdateTaxRateAsync(updateRequest), Times.Once);
        }

        [Fact]
        public async Task UpdateTaxRate_ReturnBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var urlId = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            var updateRequest = new UpdateTaxRateRequest
            {
                ID = requestId,
                Name_en = "GST",
                Name_fr = "TPS",
                Country = "Canada",
                Rate = 0.05m,
                IsActive = true
            };

            // Act
            var result = await controller.UpdateTaxRate(urlId, updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("ID in the URL does not match", badRequestResult.Value?.ToString());
            mockService.Verify(service => service.UpdateTaxRateAsync(It.IsAny<UpdateTaxRateRequest>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTaxRate_ReturnOk_WhenTaxRateExists()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var taxRateId = Guid.NewGuid();
            var deleteResponse = new DeleteTaxRateResponse
            {
                ID = taxRateId,
                Message = "Tax rate has been deactivated successfully."
            };

            mockService.Setup(service => service.DeleteTaxRateAsync(taxRateId))
                      .ReturnsAsync(Result.Success(deleteResponse));

            // Act
            var result = await controller.DeleteTaxRate(taxRateId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            mockService.Verify(service => service.DeleteTaxRateAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task DeleteTaxRate_ReturnNotFound_WhenTaxRateDoesNotExist()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var taxRateId = Guid.NewGuid();

            mockService.Setup(service => service.DeleteTaxRateAsync(taxRateId))
                      .ReturnsAsync(Result.Failure<DeleteTaxRateResponse>("Tax rate not found.", StatusCodes.Status404NotFound));

            // Act
            var result = await controller.DeleteTaxRate(taxRateId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
            mockService.Verify(service => service.DeleteTaxRateAsync(taxRateId), Times.Once);
        }

        [Fact]
        public async Task GetTaxRateById_ReturnInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var mockService = new Mock<ITaxRatesService>();
            var controller = new TaxRatesController(mockService.Object);
            var taxRateId = Guid.NewGuid();

            mockService.Setup(service => service.GetTaxRateByIdAsync(taxRateId))
                      .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await controller.GetTaxRateById(taxRateId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Contains("Database connection failed", statusCodeResult.Value?.ToString());
            mockService.Verify(service => service.GetTaxRateByIdAsync(taxRateId), Times.Once);
        }
    }
}