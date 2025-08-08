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
    public class AddressControllerShould
    {
        private readonly Mock<IAddressService> _mockAddressService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly AddressController _controller;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testAddressId = Guid.NewGuid();

        public AddressControllerShould()
        {
            _mockAddressService = new Mock<IAddressService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new AddressController(_mockAddressService.Object, _mockUserService.Object);

            // Setup user context
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Setup user service to return test user
            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                .ReturnsAsync(Result.Success(new User 
                { 
                    ID = _testUserId, 
                    Email = "test@example.com",
                    Firstname = "Test",
                    Lastname = "User",
                    Password = "hashedpassword",
                    Createdat = DateTime.UtcNow,
                    Deleted = false,
                    ValidEmail = true
                }));
        }

        [Fact]
        public async Task CreateAddress_ReturnOk_WhenAddressCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateAddressRequest
            {
                Street = "123 Main St",
                City = "Test City",
                State = "Test State",
                PostalCode = "12345",
                Country = "Test Country",
                AddressType = "Delivery"
            };

            var response = new CreateAddressResponse
            {
                Id = _testAddressId,
                UserId = _testUserId,
                Street = request.Street,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                AddressType = request.AddressType,
                CreatedAt = DateTime.UtcNow
            };

            _mockAddressService.Setup(x => x.CreateAddressAsync(It.IsAny<CreateAddressRequest>(), _testUserId))
                .ReturnsAsync(Result.Success(response));

            // Act
            var result = await _controller.CreateAddress(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<Result<CreateAddressResponse>>(okResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Equal(_testAddressId, returnedResult.Value!.Id);
        }

        [Fact]
        public async Task CreateAddress_ReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var request = new CreateAddressRequest
            {
                Street = "123 Main St",
                City = "Test City",
                PostalCode = "12345",
                Country = "Test Country",
                AddressType = "Delivery"
            };

            _controller.ModelState.AddModelError("Street", "Street is required");

            // Act
            var result = await _controller.CreateAddress(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateAddress_ReturnError_WhenServiceFails()
        {
            // Arrange
            var request = new CreateAddressRequest
            {
                Street = "123 Main St",
                City = "Test City",
                PostalCode = "12345",
                Country = "Test Country",
                AddressType = "Delivery"
            };

            _mockAddressService.Setup(x => x.CreateAddressAsync(It.IsAny<CreateAddressRequest>(), _testUserId))
                .ReturnsAsync(Result.Failure<CreateAddressResponse>("Error creating address", StatusCodes.Status500InternalServerError));

            // Act
            var result = await _controller.CreateAddress(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        }

        [Fact]
        public async Task UpdateAddress_ReturnOk_WhenAddressUpdatedSuccessfully()
        {
            // Arrange
            var request = new UpdateAddressRequest
            {
                Id = _testAddressId,
                Street = "456 Updated St",
                City = "Updated City",
                PostalCode = "54321",
                Country = "Updated Country",
                AddressType = "Billing"
            };

            var response = new UpdateAddressResponse
            {
                Id = _testAddressId,
                UserId = _testUserId,
                Street = request.Street,
                City = request.City,
                PostalCode = request.PostalCode,
                Country = request.Country,
                AddressType = request.AddressType,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _mockAddressService.Setup(x => x.UpdateAddressAsync(It.IsAny<UpdateAddressRequest>(), _testUserId))
                .ReturnsAsync(Result.Success(response));

            // Act
            var result = await _controller.UpdateAddress(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<Result<UpdateAddressResponse>>(okResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Equal(_testAddressId, returnedResult.Value!.Id);
        }

        [Fact]
        public async Task DeleteAddress_ReturnOk_WhenAddressDeletedSuccessfully()
        {
            // Arrange
            var response = new DeleteAddressResponse
            {
                Id = _testAddressId
            };

            _mockAddressService.Setup(x => x.DeleteAddressAsync(_testAddressId, _testUserId))
                .ReturnsAsync(Result.Success(response));

            // Act
            var result = await _controller.DeleteAddress(_testAddressId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<Result<DeleteAddressResponse>>(okResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Equal(_testAddressId, returnedResult.Value!.Id);
        }

        [Fact]
        public async Task DeleteAddress_ReturnBadRequest_WhenAddressIdIsEmpty()
        {
            // Act
            var result = await _controller.DeleteAddress(Guid.Empty);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetAddress_ReturnOk_WhenAddressFound()
        {
            // Arrange
            var response = new GetAddressResponse
            {
                Id = _testAddressId,
                UserId = _testUserId,
                Street = "123 Main St",
                City = "Test City",
                PostalCode = "12345",
                Country = "Test Country",
                AddressType = "Delivery",
                CreatedAt = DateTime.UtcNow
            };

            _mockAddressService.Setup(x => x.GetAddressAsync(_testAddressId, _testUserId))
                .ReturnsAsync(Result.Success(response));

            // Act
            var result = await _controller.GetAddress(_testAddressId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<Result<GetAddressResponse>>(okResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Equal(_testAddressId, returnedResult.Value!.Id);
        }

        [Fact]
        public async Task GetAddress_ReturnNotFound_WhenAddressNotFound()
        {
            // Arrange
            _mockAddressService.Setup(x => x.GetAddressAsync(_testAddressId, _testUserId))
                .ReturnsAsync(Result.Failure<GetAddressResponse>("Address not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.GetAddress(_testAddressId);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetUserAddresses_ReturnOk_WhenAddressesFound()
        {
            // Arrange
            var addresses = new List<GetAddressResponse>
            {
                new() {
                    Id = _testAddressId,
                    UserId = _testUserId,
                    Street = "123 Main St",
                    City = "Test City",
                    PostalCode = "12345",
                    Country = "Test Country",
                    AddressType = "Delivery",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockAddressService.Setup(x => x.GetUserAddressesAsync(_testUserId))
                .ReturnsAsync(Result.Success(addresses.AsEnumerable()));

            // Act
            var result = await _controller.GetUserAddresses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<Result<IEnumerable<GetAddressResponse>>>(okResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Single(returnedResult.Value!);
        }

        [Fact]
        public async Task GetUserAddressesByType_ReturnOk_WhenAddressesFound()
        {
            // Arrange
            var addressType = "Delivery";
            var addresses = new List<GetAddressResponse>
            {
                new() {
                    Id = _testAddressId,
                    UserId = _testUserId,
                    Street = "123 Main St",
                    City = "Test City",
                    PostalCode = "12345",
                    Country = "Test Country",
                    AddressType = addressType,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockAddressService.Setup(x => x.GetUserAddressesByTypeAsync(_testUserId, addressType))
                .ReturnsAsync(Result.Success(addresses.AsEnumerable()));

            // Act
            var result = await _controller.GetUserAddressesByType(addressType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<Result<IEnumerable<GetAddressResponse>>>(okResult.Value);
            Assert.True(returnedResult.IsSuccess);
            Assert.Single(returnedResult.Value!);
        }

        [Fact]
        public async Task GetUserAddressesByType_ReturnBadRequest_WhenAddressTypeIsEmpty()
        {
            // Act
            var result = await _controller.GetUserAddressesByType("");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateAddress_ReturnUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var request = new CreateAddressRequest
            {
                Street = "123 Main St",
                City = "Test City",
                PostalCode = "12345",
                Country = "Test Country",
                AddressType = "Delivery"
            };

            _mockUserService.Setup(x => x.GetUserEntityAsync("test@example.com"))
                .ReturnsAsync(Result.Failure<User>("User not found", StatusCodes.Status404NotFound));

            // Act
            var result = await _controller.CreateAddress(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, statusResult.StatusCode);
        }
    }
}