using API.Controllers;
using Domain.Services.Interfaces;
using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests
{
    public class ValidateEmailShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public ValidateEmailShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenEmailValidatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var result = Result.Success(true);
            _mockUserService.Setup(s => s.ValidateEmailAsync(userId)).ReturnsAsync(result);

            // Act
            var response = await _controller.ValidateEmail(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            _mockUserService.Verify(s => s.ValidateEmailAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var result = Result.Failure<bool>("User not found.", StatusCodes.Status404NotFound);
            _mockUserService.Setup(s => s.ValidateEmailAsync(userId)).ReturnsAsync(result);

            // Act
            var response = await _controller.ValidateEmail(userId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal("User not found.", objectResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailAlreadyValidated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var result = Result.Failure<bool>("Email is already validated.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.ValidateEmailAsync(userId)).ReturnsAsync(result);

            // Act
            var response = await _controller.ValidateEmail(userId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal("Email is already validated.", objectResult.Value);
        }
    }
}