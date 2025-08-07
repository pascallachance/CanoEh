using API.Controllers;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace API.Tests
{
    public class CompanyControllerShould
    {
        private readonly Mock<ICompanyService> _mockCompanyService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly CompanyController _controller;

        public CompanyControllerShould()
        {
            _mockCompanyService = new Mock<ICompanyService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new CompanyController(_mockCompanyService.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task CreateCompany_ReturnOk_WhenCompanyCreatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newCompany = new CreateCompanyRequest
            {
                OwnerID = userId,
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png"
            };

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

            var result = Result.Success(new CreateCompanyResponse
            {
                Id = Guid.NewGuid(),
                OwnerID = newCompany.OwnerID,
                Name = newCompany.Name,
                Description = newCompany.Description,
                Logo = newCompany.Logo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });

            _mockUserService.Setup(s => s.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(Result.Success<User?>(user));
            _mockCompanyService.Setup(s => s.CreateCompanyAsync(newCompany)).ReturnsAsync(result);

            // Set up authenticated user context
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
            var response = await _controller.CreateCompany(newCompany);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(result.Value, okResult.Value);
        }

        [Fact]
        public async Task CreateCompany_ReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange
            var newCompany = new CreateCompanyRequest
            {
                OwnerID = Guid.NewGuid(),
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png"
            };

            // Set up controller context with no authenticated user (empty claims)
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var response = await _controller.CreateCompany(newCompany);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task CreateCompany_ReturnForbidden_WhenOwnerIdDoesNotMatchAuthenticatedUser()
        {
            // Arrange
            var authenticatedUserId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            
            var newCompany = new CreateCompanyRequest
            {
                OwnerID = differentUserId, // Different from authenticated user
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png"
            };

            var user = new User
            {
                ID = authenticatedUserId,
                Email = "test@example.com",
                Firstname = "Test",
                Lastname = "User",
                Createdat = DateTime.UtcNow,
                Password = "hashedpassword",
                Deleted = false,
                ValidEmail = true
            };

            _mockUserService.Setup(s => s.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(Result.Success<User?>(user));

            // Set up authenticated user context
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
            var response = await _controller.CreateCompany(newCompany);

            // Assert
            var forbiddenResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(StatusCodes.Status403Forbidden, forbiddenResult.StatusCode);
        }

        [Fact]
        public async Task GetCompany_ReturnOk_WhenCompanyExists()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var result = Result.Success(new GetCompanyResponse
            {
                Id = companyId,
                OwnerID = Guid.NewGuid(),
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });

            _mockCompanyService.Setup(s => s.GetCompanyAsync(companyId)).ReturnsAsync(result);

            // Act
            var response = await _controller.GetCompany(companyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(result.Value, okResult.Value);
        }

        [Fact]
        public async Task GetCompany_ReturnBadRequest_WhenCompanyIdEmpty()
        {
            // Arrange
            var companyId = Guid.Empty;

            // Act
            var response = await _controller.GetCompany(companyId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetMyCompanies_ReturnOk_WhenUserAuthenticated()
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
                    Name = "Company 1",
                    Description = "First company",
                    Logo = "logo1.png",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    OwnerID = userId,
                    Name = "Company 2",
                    Description = "Second company",
                    Logo = "logo2.png",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                }
            };

            _mockUserService.Setup(s => s.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(Result.Success<User?>(user));
            _mockCompanyService.Setup(s => s.GetCompaniesByOwnerAsync(userId))
                              .ReturnsAsync(Result.Success<IEnumerable<GetCompanyResponse>>(companies));

            // Set up authenticated user context
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
            var response = await _controller.GetMyCompanies();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(companies, okResult.Value);
        }
    }
}