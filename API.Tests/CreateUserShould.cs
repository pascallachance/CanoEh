using API.Controllers;
using Domain.Models;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests
{
    public class CreateUserShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public CreateUserShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenUserCreatedSuccessfully()
        {
            // Arrange
            var newUser = new CreateUserRequest
            {
                Uname = "testuser",
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com",
                Password = "password123"
            };
            var result = Result.Success($"User {newUser.Uname} created successfully.");
            _mockUserService.Setup(s => s.CreateUserAsync(newUser)).ReturnsAsync(result);

            // Act
            var response = await _controller.CreateUser(newUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(result, okResult.Value);
        }

        [Fact]
        public async Task CreatedUser_HasSameValues_AsInputModel()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<User>>();
            var inputModel = new CreateUserRequest
            {
                Uname = "plachance",
                Firstname = "Pascal",
                Lastname = "Lachance",
                Email = "plachance@gmail.com",
                Phone = "1234567890",
                Password = "password123",
            };

            User? createdUser = null;
            mockRepo
                .Setup(repo => repo.Add(It.IsAny<User>()))
                .Returns((User u) =>
                {
                    createdUser = u;
                    return u;
                });

            var userService = new UserService(mockRepo.Object);

            // Act
            var result = await userService.CreateUserAsync(inputModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdUser);
            Assert.Equal(inputModel.Uname, createdUser.Uname);
            Assert.Equal(inputModel.Firstname, createdUser.Firstname);
            Assert.Equal(inputModel.Lastname, createdUser.Lastname);
            Assert.Equal(inputModel.Email, createdUser.Email);
            Assert.Equal(inputModel.Phone, createdUser.Phone);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenUnameNotSupplied()
        {
            var newUser = new CreateUserRequest
            {
                Uname = "",
                Firstname = "Pascal",
                Lastname = "Lachance",
                Email = "plachance@gmail.com",
                Phone = "1234567890",
                Password = "password123",
            };

            _controller.ModelState.AddModelError("Uname", "Required");

            var response = await _controller.CreateUser(newUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenFirstNameNotSupplied()
        {
            var newUser = new CreateUserRequest
            {
                Uname = "plachance",
                Firstname = "",
                Lastname = "Lachance",
                Email = "plachance@gmail.com",
                Phone = "1234567890",
                Password = "password123",
            };

            _controller.ModelState.AddModelError("FirstName", "Required");

            var response = await _controller.CreateUser(newUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenLastNameNotSupplied()
        {
            var newUser = new CreateUserRequest
            {
                Uname = "plachance",
                Firstname = "Pascal",
                Lastname = "",
                Email = "plachance@gmail.com",
                Phone = "1234567890",
                Password = "password123",
            };

            _controller.ModelState.AddModelError("LastName", "Required");

            var response = await _controller.CreateUser(newUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailNotSupplied()
        {
            var newUser = new CreateUserRequest
            {
                Uname = "plachance",
                Firstname = "Pascal",
                Lastname = "Lachance",
                Email = "",
                Phone = "1234567890",
                Password = "password123",
            };

            _controller.ModelState.AddModelError("Email", "Required");

            var response = await _controller.CreateUser(newUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task CreatedUser_IsNotDeleted()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<User>>();
            var inputModel = new CreateUserRequest
            {
                Uname = "plachance",
                Firstname = "Pascal",
                Lastname = "Lachance",
                Email = "plachance@gmail.com",
                Phone = "1234567890",
                Password = "password123",
            };

            User? createdUser = null;
            mockRepo
                .Setup(repo => repo.Add(It.IsAny<User>()))
                .Returns((User u) =>
                {
                    createdUser = u;
                    return u;
                });

            var userService = new UserService(mockRepo.Object);

            // Act
            var result = await userService.CreateUserAsync(inputModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdUser);
            Assert.False(createdUser.Deleted);
        }

        [Fact]
        public async Task ReturnErrorStatus_WhenServiceReturnsFailure()
        {
            var newUser = new CreateUserRequest
            {
                Uname = "failuser",
                Firstname = "Fail",
                Lastname = "User",
                Email = "fail@example.com",
                Password = "password123"
            };

            var result = Result.Failure("Username already exists.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.CreateUserAsync(newUser)).ReturnsAsync(result);

            var response = await _controller.CreateUser(newUser);

            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal(result.Error, objectResult.Value);
        }
    }
}


