using Domain.Models.Requests;
using Microsoft.AspNetCore.Http;

namespace API.Tests
{
    public class PaymentMethodValidationShould
    {
        [Fact]
        public void CreatePaymentMethodRequest_ReturnSuccess_WhenAllValidationsPassed()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                BillingAddress = "123 Test St",
                IsDefault = true
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreatePaymentMethodRequest_ReturnFailure_WhenTypeIsEmpty()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = false
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Payment method type is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreatePaymentMethodRequest_ReturnFailure_WhenTypeIsInvalid()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "Bitcoin", // Invalid type
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = false
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Payment method type must be 'Credit Card', 'Debit Card', 'PayPal', or 'Bank Transfer'.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreatePaymentMethodRequest_ReturnFailure_WhenCardTypeWithoutCardHolderName()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "Credit Card",
                CardHolderName = "", // Missing card holder name
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = false
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Card holder name is required for card payments.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreatePaymentMethodRequest_ReturnFailure_WhenCardTypeWithInvalidCardLast4()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "123", // Invalid - not 4 digits
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = false
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Card last 4 digits are required and must be exactly 4 digits.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreatePaymentMethodRequest_ReturnFailure_WhenCardTypeWithInvalidExpirationMonth()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 13, // Invalid - month > 12
                ExpirationYear = 2025,
                IsDefault = false
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Valid expiration month (1-12) is required for card payments.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreatePaymentMethodRequest_ReturnFailure_WhenCardTypeWithExpiredYear()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = DateTime.Now.Year - 1, // Expired year
                IsDefault = false
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Valid expiration year is required for card payments.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreatePaymentMethodRequest_ReturnSuccess_WhenPayPalTypeWithoutCardFields()
        {
            // Arrange
            var request = new CreatePaymentMethodRequest
            {
                Type = "PayPal",
                // No card fields required for PayPal
                IsDefault = false
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdatePaymentMethodRequest_ReturnSuccess_WhenAllValidationsPassed()
        {
            // Arrange
            var request = new UpdatePaymentMethodRequest
            {
                ID = Guid.NewGuid(),
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                BillingAddress = "123 Test St",
                IsDefault = true,
                IsActive = true
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdatePaymentMethodRequest_ReturnFailure_WhenIDIsEmpty()
        {
            // Arrange
            var request = new UpdatePaymentMethodRequest
            {
                ID = Guid.Empty, // Invalid - empty ID
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = false,
                IsActive = true
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Payment method ID is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }
    }
}