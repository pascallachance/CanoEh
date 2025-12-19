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
using System.Security.Claims;

namespace API.Tests
{
    public class CompanyControllerShould
    {
        private readonly Mock<ICompanyService> _mockCompanyService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly CompanyController _controller;

        public CompanyControllerShould()
        {
            _mockCompanyService = new Mock<ICompanyService>();
            _mockUserService = new Mock<IUserService>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _controller = new CompanyController(_mockCompanyService.Object, _mockUserService.Object, _mockFileStorageService.Object);
        }

        [Fact]
        public async Task CreateCompany_ReturnOk_WhenCompanyCreatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newCompany = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
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
                OwnerID = userId,
                Name = newCompany.Name,
                Description = newCompany.Description,
                Logo = newCompany.Logo,
                Email = newCompany.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });

            _mockUserService.Setup(s => s.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(Result.Success<User?>(user));
            _mockCompanyService.Setup(s => s.CreateCompanyAsync(newCompany, userId)).ReturnsAsync(result);

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
                Name = "Test Company",
                Email = "test@company.com",
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
        public async Task CreateCompany_ReturnUnauthorized_WhenUserDoesNotExist()
        {
            // Arrange
            var newCompany = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
                Description = "A test company",
                Logo = "test-logo.png"
            };

            _mockUserService.Setup(s => s.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(Result.Failure<User?>("User not found.", StatusCodes.Status401Unauthorized));

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
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(response);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
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
                Email = "test@company.com",
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
        public async Task CreateCompany_UsesAuthenticatedUserIdAsOwner()
        {
            // Arrange
            var authenticatedUserId = Guid.NewGuid();
            var newCompany = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
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

            var result = Result.Success(new CreateCompanyResponse
            {
                Id = Guid.NewGuid(),
                OwnerID = authenticatedUserId, // This should be the authenticated user's ID
                Name = newCompany.Name,
                Description = newCompany.Description,
                Logo = newCompany.Logo,
                Email = newCompany.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });

            _mockUserService.Setup(s => s.GetUserEntityAsync("test@example.com"))
                           .ReturnsAsync(Result.Success<User?>(user));
            _mockCompanyService.Setup(s => s.CreateCompanyAsync(newCompany, authenticatedUserId))
                              .ReturnsAsync(result);

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
            var createdCompany = Assert.IsType<CreateCompanyResponse>(okResult.Value);
            
            // Verify that the OwnerID matches the authenticated user's ID
            Assert.Equal(authenticatedUserId, createdCompany.OwnerID);
            
            // Verify that the service was called with the correct authenticated user ID
            _mockCompanyService.Verify(s => s.CreateCompanyAsync(newCompany, authenticatedUserId), Times.Once);
        }

        [Fact]
        public async Task GetMyCompany_ReturnOk_WhenUserAuthenticated()
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
                    Email = "company1@example.com",
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
                    Email = "company2@example.com",
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
            var response = await _controller.GetMyCompany();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(companies, okResult.Value);
        }
    }
}