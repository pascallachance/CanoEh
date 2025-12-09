using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Infrastructure.Services;
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
                Email = "testuser@test.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "password123"
            };
            var result = Result.Success(new CreateUserResponse 
            { 
                ID = Guid.NewGuid(),
                Firstname = newUser.Firstname,
                Lastname = newUser.Lastname,
                Email = newUser.Email,
                Phone = newUser.Phone,
                Lastlogin = null,
                CreatedAt = DateTime.UtcNow,
                LastupdatedAt = null,
                Deleted = false,
                ValidEmail = false
            });
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
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var inputModel = new CreateUserRequest
            {
                Email = "plachance@gmail.com",
                Firstname = "Pascal",
                Lastname = "Lachance",
                Phone = "1234567890",
                Password = "password123",
            };

            User? createdUser = null;
            mockRepo
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    createdUser = u;
                    return u;
                });

            mockEmailService
                .Setup(es => es.SendEmailValidationAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success());

            var userService = new UserService(mockRepo.Object, mockEmailService.Object);

            // Act
            var result = await userService.CreateUserAsync(inputModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdUser);
            Assert.Equal(inputModel.Email, createdUser.Email);
            Assert.Equal(inputModel.Firstname, createdUser.Firstname);
            Assert.Equal(inputModel.Lastname, createdUser.Lastname);
            Assert.Equal(inputModel.Email, createdUser.Email);
            Assert.Equal(inputModel.Phone, createdUser.Phone);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailNotSupplied()
        {
            var newUser = new CreateUserRequest
            {
                Email = "",
                Firstname = "Pascal",
                Lastname = "Lachance",
                Phone = "1234567890",
                Password = "password123",
            };

            _controller.ModelState.AddModelError("Email", "Required");

            var response = await _controller.CreateUser(newUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenFirstNameNotSupplied()
        {
            var newUser = new CreateUserRequest
            {
                Email = "plachance@gmail.com",
                Firstname = "Test",
                Lastname = "User",
                Phone = "1234567890",
                Password = "password123"
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
                Email = "plachance@gmail.com",
                Firstname = "Test",
                Lastname = "User",
                Phone = "1234567890",
                Password = "password123"
            };

            _controller.ModelState.AddModelError("LastName", "Required");

            var response = await _controller.CreateUser(newUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailIsEmpty()
        {
            var newUser = new CreateUserRequest
            {
                Email = "",
                Firstname = "Pascal",
                Lastname = "Lachance",
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
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var inputModel = new CreateUserRequest
            {
                Email = "plachance@gmail.com",
                Firstname = "Test",
                Lastname = "User",
                Phone = "1234567890",
                Password = "password123"
            };

            User? createdUser = null;
            mockRepo
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    createdUser = u;
                    return u;
                });
            mockRepo
                .Setup(repo => repo.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null); // No existing user found

            mockEmailService
                .Setup(es => es.SendEmailValidationAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success());

            var userService = new UserService(mockRepo.Object, mockEmailService.Object);

            // Act
            var result = await userService.CreateUserAsync(inputModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdUser);
            Assert.False(createdUser.Deleted);
        }

        [Fact]
        public async Task CreatedUser_HasValidEmailSetToFalse()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var inputModel = new CreateUserRequest
            {
                Email = "plachance@gmail.com",
                Firstname = "Test",
                Lastname = "User",
                Phone = "1234567890",
                Password = "password123"
            };

            User? createdUser = null;
            mockRepo
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    createdUser = u;
                    return u;
                });
            mockRepo
                .Setup(repo => repo.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null); // No existing user found

            mockEmailService
                .Setup(es => es.SendEmailValidationAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success());

            var userService = new UserService(mockRepo.Object, mockEmailService.Object);

            // Act
            var result = await userService.CreateUserAsync(inputModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdUser);
            Assert.False(createdUser.ValidEmail);
            mockEmailService.Verify(es => es.SendEmailValidationAsync(
                It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ReturnErrorStatus_WhenServiceReturnsFailure()
        {
            var newUser = new CreateUserRequest
            {
                Email = "fail@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "password123"
            };

            var result = Result.Failure<CreateUserResponse>("Username already exists.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.CreateUserAsync(newUser)).ReturnsAsync(result);

            var response = await _controller.CreateUser(newUser);

            var objectResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal(result.Error, objectResult.Value);
        }

        [Fact]
        public async Task CreateUser_WithSpecificLanguage_PersistsLanguageCorrectly()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var inputModel = new CreateUserRequest
            {
                Email = "test@example.com",
                Firstname = "Jean",
                Lastname = "Dupont",
                Password = "password123",
                Language = "fr"
            };

            User? createdUser = null;
            mockRepo
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    createdUser = u;
                    return u;
                });

            mockEmailService
                .Setup(es => es.SendEmailValidationAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success());

            var userService = new UserService(mockRepo.Object, mockEmailService.Object);

            // Act
            var result = await userService.CreateUserAsync(inputModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdUser);
            Assert.Equal("fr", createdUser.Language);
        }

        [Fact]
        public async Task CreateUser_WithoutSpecifyingLanguage_DefaultsToEnglish()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var mockEmailService = new Mock<IEmailService>();
            var inputModel = new CreateUserRequest
            {
                Email = "test@example.com",
                Firstname = "John",
                Lastname = "Doe",
                Password = "password123"
                // Language not specified, should default to "en"
            };

            User? createdUser = null;
            mockRepo
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) =>
                {
                    createdUser = u;
                    return u;
                });

            mockEmailService
                .Setup(es => es.SendEmailValidationAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success());

            var userService = new UserService(mockRepo.Object, mockEmailService.Object);

            // Act
            var result = await userService.CreateUserAsync(inputModel);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdUser);
            Assert.Equal("en", createdUser.Language);
        }

        [Fact]
        public void CreateUserRequest_Validate_RejectInvalidLanguageCode()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "password123",
                Language = "es" // Spanish - not supported
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Language must be 'en' or 'fr'", result.Error);
        }

        [Fact]
        public void CreateUserRequest_Validate_RejectLanguageCodeTooLong()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "password123",
                Language = "verylonglanguagecode" // Too long
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Language code must not exceed 10 characters", result.Error);
        }

        [Fact]
        public void CreateUserRequest_Validate_AcceptValidLanguageCodes()
        {
            // Arrange
            var requestEn = new CreateUserRequest
            {
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "password123",
                Language = "en"
            };

            var requestFr = new CreateUserRequest
            {
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Password = "password123",
                Language = "fr"
            };

            // Act
            var resultEn = requestEn.Validate();
            var resultFr = requestFr.Validate();

            // Assert
            Assert.True(resultEn.IsSuccess);
            Assert.True(resultFr.IsSuccess);
        }
    }
}


