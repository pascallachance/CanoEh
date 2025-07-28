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
        private readonly EmailValidationController _controller;

        public ValidateEmailShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new EmailValidationController(_mockUserService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenEmailValidatedSuccessfully()
        {
            // Arrange
            var token = "valid-token-123";
            var result = Result.Success(true);
            _mockUserService.Setup(s => s.ValidateEmailByTokenAsync(token)).ReturnsAsync(result);

            // Act
            var response = await _controller.Index(token);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(response);
            Assert.Equal("ValidateEmail", viewResult.ViewName);
            _mockUserService.Verify(s => s.ValidateEmailByTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task ReturnNotFound_WhenUserNotFound()
        {
            // Arrange
            var token = "invalid-token-123";
            var result = Result.Failure<bool>("Invalid or expired validation token.", StatusCodes.Status404NotFound);
            _mockUserService.Setup(s => s.ValidateEmailByTokenAsync(token)).ReturnsAsync(result);

            // Act
            var response = await _controller.Index(token);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(response);
            Assert.Equal("ValidateEmail", viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);
            Assert.True(viewResult.ViewData.ContainsKey("Message"));
            Assert.Contains("Invalid or expired validation link", viewResult.ViewData["Message"]?.ToString());
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailAlreadyValidated()
        {
            // Arrange
            var token = "used-token-123";
            var result = Result.Failure<bool>("Email is already validated.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.ValidateEmailByTokenAsync(token)).ReturnsAsync(result);

            // Act
            var response = await _controller.Index(token);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(response);
            Assert.Equal("ValidateEmail", viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);
            Assert.True(viewResult.ViewData.ContainsKey("Message"));
            Assert.Contains("This email address has already been validated", viewResult.ViewData["Message"]?.ToString());
        }
    }
}