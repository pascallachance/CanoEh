using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests
{
    public class UpdateUserShould
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UpdateUserShould()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [Fact]
        public async Task ReturnOk_WhenUserUpdatedSuccessfully()
        {
            // Arrange
            var username = "testuser";
            var updateRequest = new UpdateUserRequest
            {
                Username = username,
                Firstname = "UpdatedFirst",
                Lastname = "UpdatedLast", 
                Email = "updated@example.com",
                Phone = "9876543210"
            };

            var updateResponse = new UpdateUserResponse
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Firstname = updateRequest.Firstname,
                Lastname = updateRequest.Lastname,
                Email = updateRequest.Email,
                Phone = updateRequest.Phone,
                Lastlogin = null,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastupdatedAt = DateTime.UtcNow,
                Deleted = false
            };

            var result = Result.Success(updateResponse);
            _mockUserService.Setup(s => s.UpdateUserAsync(updateRequest)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.UpdateUser(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(updateResponse, okResult.Value);
        }

        [Fact]
        public async Task ReturnForbidden_WhenUserTriesToUpdateAnotherUser()
        {
            // Arrange
            var targetUsername = "otheruser";
            var authenticatedUsername = "testuser";
            var updateRequest = new UpdateUserRequest
            {
                Username = targetUsername,
                Firstname = "UpdatedFirst",
                Lastname = "UpdatedLast",
                Email = "updated@example.com"
            };

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, authenticatedUsername) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.UpdateUser(updateRequest);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status403Forbidden, forbiddenResult.StatusCode);
            Assert.Equal("You can only update your own user information.", forbiddenResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenUsernameIsEmpty()
        {
            // Arrange
            var username = "";
            var updateRequest = new UpdateUserRequest
            {
                Username = username,
                Firstname = "Test",
                Lastname = "User", 
                Email = "test@example.com"
            };

            var result = Result.Failure<UpdateUserResponse>("Username is required.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.UpdateUserAsync(updateRequest)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.UpdateUser(updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal(result.Error, badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenFirstnameNotSupplied()
        {
            // Arrange
            var username = "testuser";
            var updateRequest = new UpdateUserRequest
            {
                Username = username,
                Firstname = "",
                Lastname = "User",
                Email = "test@example.com"
            };

            var result = Result.Failure<UpdateUserResponse>("First name is required.", StatusCodes.Status400BadRequest);
            _mockUserService.Setup(s => s.UpdateUserAsync(updateRequest)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.UpdateUser(updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            Assert.Equal(result.Error, badRequestResult.Value);
        }

        [Fact]
        public async Task ReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "nonexistentuser";
            var updateRequest = new UpdateUserRequest
            {
                Username = username,
                Firstname = "Test",
                Lastname = "User",
                Email = "test@example.com"
            };

            var result = Result.Failure<UpdateUserResponse>("User not found.", StatusCodes.Status404NotFound);
            _mockUserService.Setup(s => s.UpdateUserAsync(updateRequest)).ReturnsAsync(result);

            // Setup authenticated user context
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var response = await _controller.UpdateUser(updateRequest);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            Assert.Equal(result.Error, notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateUser_SetsLastUpdatedAt_ToCurrentTime()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<User>>();
            var username = "testuser";
            var updateRequest = new UpdateUserRequest
            {
                Username = username,
                Firstname = "UpdatedFirst",
                Lastname = "UpdatedLast",
                Email = "updated@example.com",
                Phone = "9876543210"
            };

            var existingUser = new User
            {
                ID = Guid.NewGuid(),
                Uname = username,
                Firstname = "Original",
                Lastname = "User",
                Email = "original@example.com",
                Phone = "1234567890",
                Lastlogin = null,
                Createdat = DateTime.UtcNow.AddDays(-30),
                Lastupdatedat = null,
                Password = "hashedpassword",
                Deleted = false
            };

            User? updatedUser = null;
            var timeBeforeUpdate = DateTime.UtcNow;

            mockRepo.Setup(repo => repo.Find(It.IsAny<Func<User, bool>>()))
                   .Returns(new List<User> { existingUser });

            mockRepo.Setup(repo => repo.Update(It.IsAny<User>()))
                   .Returns((User u) =>
                   {
                       updatedUser = u;
                       return u;
                   });

            var userService = new UserService(mockRepo.Object);

            // Act
            var result = await userService.UpdateUserAsync(updateRequest);
            var timeAfterUpdate = DateTime.UtcNow;

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(updatedUser);
            Assert.NotNull(updatedUser.Lastupdatedat);
            Assert.True(updatedUser.Lastupdatedat >= timeBeforeUpdate);
            Assert.True(updatedUser.Lastupdatedat <= timeAfterUpdate);
            Assert.Equal(updateRequest.Firstname, updatedUser.Firstname);
            Assert.Equal(updateRequest.Lastname, updatedUser.Lastname);
            Assert.Equal(updateRequest.Email, updatedUser.Email);
            Assert.Equal(updateRequest.Phone, updatedUser.Phone);
            
            // Ensure immutable fields are not changed
            Assert.Equal(existingUser.ID, updatedUser.ID);
            Assert.Equal(existingUser.Uname, updatedUser.Uname);
            Assert.Equal(existingUser.Createdat, updatedUser.Createdat);
            Assert.Equal(existingUser.Lastlogin, updatedUser.Lastlogin);
            Assert.Equal(existingUser.Password, updatedUser.Password);
        }

        [Fact]
        public async Task UpdateUser_DoesNotModify_ImmutableFields()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<User>>();
            var username = "testuser";
            var updateRequest = new UpdateUserRequest
            {
                Username = username,
                Firstname = "UpdatedFirst",
                Lastname = "UpdatedLast",
                Email = "updated@example.com"
            };

            var originalId = Guid.NewGuid();
            var originalCreateDate = DateTime.UtcNow.AddDays(-30);
            var originalLastLogin = DateTime.UtcNow.AddDays(-1);
            var originalPassword = "originalhashedpassword";

            var existingUser = new User
            {
                ID = originalId,
                Uname = username,
                Firstname = "Original",
                Lastname = "User",
                Email = "original@example.com",
                Phone = "1234567890",
                Lastlogin = originalLastLogin,
                Createdat = originalCreateDate,
                Lastupdatedat = null,
                Password = originalPassword,
                Deleted = false
            };

            User? updatedUser = null;

            mockRepo.Setup(repo => repo.Find(It.IsAny<Func<User, bool>>()))
                   .Returns([existingUser]);

            mockRepo.Setup(repo => repo.Update(It.IsAny<User>()))
                   .Returns((User u) =>
                   {
                       updatedUser = u;
                       return u;
                   });

            var userService = new UserService(mockRepo.Object);

            // Act
            var result = await userService.UpdateUserAsync(updateRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(updatedUser);
            
            // Verify immutable fields remain unchanged
            Assert.Equal(originalId, updatedUser.ID);
            Assert.Equal(username, updatedUser.Uname);
            Assert.Equal(originalCreateDate, updatedUser.Createdat);
            Assert.Equal(originalLastLogin, updatedUser.Lastlogin);
            Assert.Equal(originalPassword, updatedUser.Password);
        }

        [Fact]
        public async Task ReturnBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<User>>();
            var username = "testuser";
            var updateRequest = new UpdateUserRequest
            {
                Username = username,
                Firstname = "Test",
                Lastname = "User",
                Email = "invalidemail" // Missing @
            };

            var userService = new UserService(mockRepo.Object);

            // Act
            var result = await userService.UpdateUserAsync(updateRequest);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
            Assert.Contains("Email must contain '@'", result.Error);
        }

    }
}