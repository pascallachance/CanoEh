using API.Controllers;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace API.Tests
{
    public class JwtCookieAuthenticationShould
    {
        private readonly Mock<ICompanyService> _mockCompanyService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly CompanyController _controller;

        public JwtCookieAuthenticationShould()
        {
            _mockCompanyService = new Mock<ICompanyService>();
            _mockUserService = new Mock<IUserService>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _controller = new CompanyController(_mockCompanyService.Object, _mockUserService.Object, _mockFileStorageService.Object);
        }

        [Fact]
        public async Task GetMyCompany_ReturnOk_WhenAuthenticatedViaCookie()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                ID = userId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Createdat = DateTime.UtcNow,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true
            };

            var companies = new List<GetCompanyResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    OwnerID = userId,
                    Name = "Test Company",
                    Description = "A test company",
                    Logo = "logo.png",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                }
            };

            _mockUserService.Setup(s => s.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(Result.Success<User?>(user));
            _mockCompanyService.Setup(s => s.GetCompaniesByOwnerAsync(userId))
                              .ReturnsAsync(Result.Success<IEnumerable<GetCompanyResponse>>(companies));

            // Set up authenticated user context (simulating cookie-based authentication)
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetMyCompany();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(companies, okResult.Value);
        }

        [Fact]
        public async Task GetMyCompany_ReturnUnauthorized_WhenNotAuthenticated()
        {
            // Arrange - set up empty authentication context
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var response = await _controller.GetMyCompany();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
            Assert.Equal("User not authenticated.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetMyCompany_ReturnUnauthorized_WhenInvalidUser()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserEntityAsync("invalid@example.com"))
                           .ReturnsAsync(Result.Failure<User?>("User not found", StatusCodes.Status404NotFound));

            // Set up authenticated user context with invalid user
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "invalid@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.GetMyCompany();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
            Assert.Equal("Invalid user.", unauthorizedResult.Value);
        }
    }
}