using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests
{
    public class PaymentMethodControllerShould
    {
        private readonly Mock<IPaymentMethodService> _mockPaymentMethodService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly PaymentMethodController _controller;

        public PaymentMethodControllerShould()
        {
            _mockPaymentMethodService = new Mock<IPaymentMethodService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new PaymentMethodController(_mockPaymentMethodService.Object, _mockUserService.Object);

            // Setup authenticated user context
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test@example.com")
            }, "test"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreatePaymentMethod_ReturnOk_WhenPaymentMethodCreatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createRequest = new CreatePaymentMethodRequest
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

            var user = new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true
            };

            var expectedResponse = new CreatePaymentMethodResponse
            {
                ID = Guid.NewGuid(),
                UserID = userId,
                Type = createRequest.Type,
                CardHolderName = createRequest.CardHolderName,
                CardLast4 = createRequest.CardLast4,
                CardBrand = createRequest.CardBrand,
                ExpirationMonth = createRequest.ExpirationMonth,
                ExpirationYear = createRequest.ExpirationYear,
                BillingAddress = createRequest.BillingAddress,
                IsDefault = createRequest.IsDefault,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                .ReturnsAsync(Result.Success(user));
            _mockPaymentMethodService.Setup(x => x.CreatePaymentMethodAsync(userId, createRequest))
                .ReturnsAsync(Result.Success(expectedResponse));

            // Act
            var result = await _controller.CreatePaymentMethod(createRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<CreatePaymentMethodResponse>(okResult.Value);
            Assert.Equal(expectedResponse.Type, response.Type);
            Assert.Equal(expectedResponse.CardHolderName, response.CardHolderName);
            Assert.Equal(expectedResponse.CardLast4, response.CardLast4);
            Assert.Equal(expectedResponse.IsDefault, response.IsDefault);
        }

        [Fact]
        public async Task CreatePaymentMethod_ReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var createRequest = new CreatePaymentMethodRequest
            {
                Type = "", // Invalid - empty type
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = false
            };

            // Add model state error to simulate validation failure
            _controller.ModelState.AddModelError("Type", "Type is required");

            // Act
            var result = await _controller.CreatePaymentMethod(createRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetPaymentMethod_ReturnOk_WhenPaymentMethodExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var paymentMethodId = Guid.NewGuid();

            var user = new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true
            };

            var expectedResponse = new GetPaymentMethodResponse
            {
                ID = paymentMethodId,
                UserID = userId,
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                BillingAddress = "123 Test St",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                .ReturnsAsync(Result.Success(user));
            _mockPaymentMethodService.Setup(x => x.GetPaymentMethodAsync(userId, paymentMethodId))
                .ReturnsAsync(Result.Success(expectedResponse));

            // Act
            var result = await _controller.GetPaymentMethod(paymentMethodId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetPaymentMethodResponse>(okResult.Value);
            Assert.Equal(expectedResponse.ID, response.ID);
            Assert.Equal(expectedResponse.Type, response.Type);
            Assert.Equal(expectedResponse.CardLast4, response.CardLast4);
        }

        [Fact]
        public async Task GetPaymentMethod_ReturnNotFound_WhenPaymentMethodDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var paymentMethodId = Guid.NewGuid();

            var user = new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true
            };

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                .ReturnsAsync(Result.Success(user));
            _mockPaymentMethodService.Setup(x => x.GetPaymentMethodAsync(userId, paymentMethodId))
                .ReturnsAsync(Result.Failure<GetPaymentMethodResponse>("Payment method not found.", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.GetPaymentMethod(paymentMethodId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeletePaymentMethod_ReturnOk_WhenPaymentMethodDeletedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var paymentMethodId = Guid.NewGuid();

            var user = new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true
            };

            var expectedResponse = new DeletePaymentMethodResponse
            {
                Success = true,
                Message = "Payment method deleted successfully.",
                PaymentMethodID = paymentMethodId
            };

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                .ReturnsAsync(Result.Success(user));
            _mockPaymentMethodService.Setup(x => x.DeletePaymentMethodAsync(userId, paymentMethodId))
                .ReturnsAsync(Result.Success(expectedResponse));

            // Act
            var result = await _controller.DeletePaymentMethod(paymentMethodId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<DeletePaymentMethodResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(paymentMethodId, response.PaymentMethodID);
        }

        [Fact]
        public async Task SetDefaultPaymentMethod_ReturnOk_WhenSuccessfullySetAsDefault()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var paymentMethodId = Guid.NewGuid();

            var user = new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "hashedpassword",
                Createdat = DateTime.UtcNow,
                Deleted = false,
                ValidEmail = true
            };

            var expectedResponse = new GetPaymentMethodResponse
            {
                ID = paymentMethodId,
                UserID = userId,
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                .ReturnsAsync(Result.Success(user));
            _mockPaymentMethodService.Setup(x => x.SetDefaultPaymentMethodAsync(userId, paymentMethodId))
                .ReturnsAsync(Result.Success(expectedResponse));

            // Act
            var result = await _controller.SetDefaultPaymentMethod(paymentMethodId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetPaymentMethodResponse>(okResult.Value);
            Assert.True(response.IsDefault);
            Assert.Equal(paymentMethodId, response.ID);
        }

        [Fact]
        public async Task CreatePaymentMethod_ReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var createRequest = new CreatePaymentMethodRequest
            {
                Type = "Credit Card",
                CardHolderName = "Test User",
                CardLast4 = "1234",
                CardBrand = "Visa",
                ExpirationMonth = 12,
                ExpirationYear = 2025,
                IsDefault = false
            };

            // Setup unauthenticated user context
            var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = unauthenticatedUser }
            };

            // Act
            var result = await _controller.CreatePaymentMethod(createRequest);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }
    }
}