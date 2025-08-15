using Domain.Models.Requests;
using Microsoft.AspNetCore.Http;

namespace API.Tests
{
    public class CreateOrderAddressValidationShould
    {
        [Fact]
        public void CreateOrderAddressRequest_ReturnSuccess_WhenAllFieldsAreValid()
        {
            // Arrange
            var request = new CreateOrderAddressRequest
            {
                FullName = "John Doe",
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                City = "Toronto",
                ProvinceState = "Ontario",
                PostalCode = "M5V 3A8",
                Country = "Canada"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateOrderAddressRequest_ReturnFailure_WhenProvinceStateIsEmpty()
        {
            // Arrange
            var request = new CreateOrderAddressRequest
            {
                FullName = "John Doe",
                AddressLine1 = "123 Main St",
                City = "Toronto",
                ProvinceState = "",
                PostalCode = "M5V 3A8",
                Country = "Canada"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Province/State is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreateOrderAddressRequest_ReturnFailure_WhenProvinceStateIsWhitespace()
        {
            // Arrange
            var request = new CreateOrderAddressRequest
            {
                FullName = "John Doe",
                AddressLine1 = "123 Main St",
                City = "Toronto",
                ProvinceState = "   ",
                PostalCode = "M5V 3A8",
                Country = "Canada"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Province/State is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }
    }
}