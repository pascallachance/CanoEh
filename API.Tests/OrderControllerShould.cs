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
    public class OrderControllerShould
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly OrderController _controller;

        public OrderControllerShould()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new OrderController(_mockOrderService.Object, _mockUserService.Object);

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
        public async Task CreateOrder_ReturnOk_WhenOrderCreatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createRequest = new CreateOrderRequest
            {
                OrderItems = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        ItemID = Guid.NewGuid(),
                        ItemVariantID = Guid.NewGuid(),
                        Quantity = 2
                    }
                },
                ShippingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Anytown",
                    ProvinceState = "New York",
                    PostalCode = "12345",
                    Country = "USA"
                },
                BillingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Anytown",
                    ProvinceState = "New York",
                    PostalCode = "12345",
                    Country = "USA"
                },
                Payment = new CreateOrderPaymentRequest
                {
                    PaymentMethodID = Guid.NewGuid(),
                    Provider = "Stripe"
                },
                Notes = "Test order"
            };

            var createResponse = new CreateOrderResponse
            {
                ID = Guid.NewGuid(),
                UserID = userId,
                OrderNumber = 1001,
                OrderDate = DateTime.UtcNow,
                StatusCode = "Pending",
                StatusName_en = "Pending",
                StatusName_fr = "En attente",
                Subtotal = 50.00m,
                TaxTotal = 6.50m,
                ShippingTotal = 10.00m,
                GrandTotal = 66.50m,
                Notes = "Test order",
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItemResponse>(),
                Addresses = new List<OrderAddressResponse>(),
                Payment = new OrderPaymentResponse()
            };

            var userResult = Result.Success(new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Createdat = DateTime.UtcNow,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true
            });

            var orderResult = Result.Success(createResponse);

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(userResult);
            _mockOrderService.Setup(x => x.CreateOrderAsync(userId, It.IsAny<CreateOrderRequest>()))
                            .ReturnsAsync(orderResult);

            // Act
            var response = await _controller.CreateOrder(createRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnedOrder = Assert.IsType<CreateOrderResponse>(okResult.Value);
            Assert.Equal(createResponse.ID, returnedOrder.ID);
            Assert.Equal(createResponse.OrderNumber, returnedOrder.OrderNumber);
            Assert.Equal(createResponse.StatusCode, returnedOrder.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_ReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var createRequest = new CreateOrderRequest
            {
                OrderItems = new List<CreateOrderItemRequest>(), // Empty items should fail validation
                ShippingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Anytown",
                    ProvinceState = "New York",
                    PostalCode = "12345",
                    Country = "USA"
                },
                BillingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Anytown",
                    ProvinceState = "New York",
                    PostalCode = "12345",
                    Country = "USA"
                },
                Payment = new CreateOrderPaymentRequest
                {
                    Provider = "Stripe"
                }
            };

            var userId = Guid.NewGuid();
            var userResult = Result.Success(new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Createdat = DateTime.UtcNow,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true
            });

            var orderResult = Result.Failure<CreateOrderResponse>("At least one order item is required.", StatusCodes.Status400BadRequest);

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(userResult);
            _mockOrderService.Setup(x => x.CreateOrderAsync(userId, It.IsAny<CreateOrderRequest>()))
                            .ReturnsAsync(orderResult);

            // Act
            var response = await _controller.CreateOrder(createRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetOrder_ReturnOk_WhenOrderExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            
            var getResponse = new GetOrderResponse
            {
                ID = orderId,
                UserID = userId,
                OrderNumber = 1001,
                OrderDate = DateTime.UtcNow,
                StatusCode = "Pending",
                StatusName_en = "Pending",
                StatusName_fr = "En attente",
                Subtotal = 50.00m,
                TaxTotal = 6.50m,
                ShippingTotal = 10.00m,
                GrandTotal = 66.50m,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItemResponse>(),
                Addresses = new List<OrderAddressResponse>(),
                Payment = new OrderPaymentResponse()
            };

            var userResult = Result.Success(new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Createdat = DateTime.UtcNow,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true
            });

            var orderResult = Result.Success(getResponse);

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(userResult);
            _mockOrderService.Setup(x => x.GetOrderAsync(userId, orderId))
                            .ReturnsAsync(orderResult);

            // Act
            var response = await _controller.GetOrder(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnedOrder = Assert.IsType<GetOrderResponse>(okResult.Value);
            Assert.Equal(getResponse.ID, returnedOrder.ID);
            Assert.Equal(getResponse.OrderNumber, returnedOrder.OrderNumber);
        }

        [Fact]
        public async Task GetOrder_ReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var userResult = Result.Success(new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Createdat = DateTime.UtcNow,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true
            });

            var orderResult = Result.Failure<GetOrderResponse>("Order not found.", StatusCodes.Status404NotFound);

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(userResult);
            _mockOrderService.Setup(x => x.GetOrderAsync(userId, orderId))
                            .ReturnsAsync(orderResult);

            // Act
            var response = await _controller.GetOrder(orderId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task DeleteOrder_ReturnOk_WhenOrderDeletedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var deleteResponse = new DeleteOrderResponse
            {
                ID = orderId,
                OrderNumber = 1001,
                Message = "Order deleted successfully."
            };

            var userResult = Result.Success(new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Createdat = DateTime.UtcNow,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true
            });

            var orderResult = Result.Success(deleteResponse);

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(userResult);
            _mockOrderService.Setup(x => x.DeleteOrderAsync(userId, orderId))
                            .ReturnsAsync(orderResult);

            // Act
            var response = await _controller.DeleteOrder(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnedResponse = Assert.IsType<DeleteOrderResponse>(okResult.Value);
            Assert.Equal(deleteResponse.ID, returnedResponse.ID);
            Assert.Equal(deleteResponse.OrderNumber, returnedResponse.OrderNumber);
            Assert.Contains("deleted successfully", returnedResponse.Message);
        }
    }
}