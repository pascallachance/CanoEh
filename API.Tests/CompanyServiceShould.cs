using Domain.Models.Requests;
using Domain.Services.Implementations;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace API.Tests
{
    public class CompanyServiceShould
    {
        private readonly Mock<ICompanyRepository> _mockCompanyRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly CompanyService _companyService;

        public CompanyServiceShould()
        {
            _mockCompanyRepository = new Mock<ICompanyRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _companyService = new CompanyService(_mockCompanyRepository.Object, _mockUserRepository.Object);
        }

        [Fact]
        public async Task CreateCompanyAsync_ReturnSuccess_WhenValidRequest()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png"
            };

            var expectedCompany = new Company
            {
                Id = Guid.NewGuid(),
                OwnerID = ownerId,
                Name = request.Name,
                Description = request.Description,
                Logo = request.Logo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _mockUserRepository.Setup(r => r.ExistsAsync(ownerId)).ReturnsAsync(true);
            _mockCompanyRepository.Setup(r => r.FindByNameAsync(request.Name)).ReturnsAsync((Company?)null);
            _mockCompanyRepository.Setup(r => r.AddAsync(It.IsAny<Company>())).ReturnsAsync(expectedCompany);

            // Act
            var result = await _companyService.CreateCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(request.Name, result.Value.Name);
            Assert.Equal(ownerId, result.Value.OwnerID);
            Assert.Equal(request.Description, result.Value.Description);
            Assert.Equal(request.Logo, result.Value.Logo);
        }

        [Fact]
        public async Task CreateCompanyAsync_ReturnFailure_WhenOwnerDoesNotExist()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png"
            };

            _mockUserRepository.Setup(r => r.ExistsAsync(ownerId)).ReturnsAsync(false);

            // Act
            var result = await _companyService.CreateCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Owner not found.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task CreateCompanyAsync_ReturnFailure_WhenCompanyNameAlreadyExists()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png"
            };

            var existingCompany = new Company
            {
                Id = Guid.NewGuid(),
                OwnerID = Guid.NewGuid(),
                Name = request.Name,
                Description = "Different description",
                Logo = "different-logo.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _mockUserRepository.Setup(r => r.ExistsAsync(ownerId)).ReturnsAsync(true);
            _mockCompanyRepository.Setup(r => r.FindByNameAsync(request.Name.Trim())).ReturnsAsync(existingCompany);

            // Act
            var result = await _companyService.CreateCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Company name already exists.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task CreateCompanyAsync_ReturnFailure_WhenCompanyNameWithWhitespaceAlreadyExists()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var request = new CreateCompanyRequest
            {
                Name = "  Test Company  ", // Name with leading and trailing whitespace
                Description = "A test company",
                Logo = "test-logo.png"
            };

            var existingCompany = new Company
            {
                Id = Guid.NewGuid(),
                OwnerID = Guid.NewGuid(),
                Name = "Test Company", // Existing company with trimmed name
                Description = "Different description",
                Logo = "different-logo.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _mockUserRepository.Setup(r => r.ExistsAsync(ownerId)).ReturnsAsync(true);
            _mockCompanyRepository.Setup(r => r.FindByNameAsync(request.Name.Trim())).ReturnsAsync(existingCompany);

            // Act
            var result = await _companyService.CreateCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Company name already exists.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task CreateCompanyAsync_TrimWhitespaceFromCompanyName_WhenCreating()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var request = new CreateCompanyRequest
            {
                Name = "  Test Company  ", // Name with leading and trailing whitespace
                Description = "A test company",
                Logo = "test-logo.png"
            };

            var expectedCompany = new Company
            {
                Id = Guid.NewGuid(),
                OwnerID = ownerId,
                Name = "Test Company", // Expected trimmed name
                Description = request.Description,
                Logo = request.Logo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _mockUserRepository.Setup(r => r.ExistsAsync(ownerId)).ReturnsAsync(true);
            _mockCompanyRepository.Setup(r => r.FindByNameAsync("Test Company")).ReturnsAsync((Company?)null);
            _mockCompanyRepository.Setup(r => r.AddAsync(It.Is<Company>(c => c.Name == "Test Company"))).ReturnsAsync(expectedCompany);

            // Act
            var result = await _companyService.CreateCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Test Company", result.Value.Name); // Should be trimmed
            _mockCompanyRepository.Verify(r => r.AddAsync(It.Is<Company>(c => c.Name == "Test Company")), Times.Once);
        }

        [Fact]
        public async Task CreateCompanyAsync_ReturnFailure_WhenOwnerIdIsEmpty()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png"
            };

            // Act
            var result = await _companyService.CreateCompanyAsync(request, Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Owner ID is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task GetCompanyAsync_ReturnSuccess_WhenCompanyExists()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var company = new Company
            {
                Id = companyId,
                OwnerID = Guid.NewGuid(),
                Name = "Test Company",
                Description = "A test company",
                Logo = "test-logo.png",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _mockCompanyRepository.Setup(r => r.GetByIdAsync(companyId)).ReturnsAsync(company);

            // Act
            var result = await _companyService.GetCompanyAsync(companyId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(company.Id, result.Value.Id);
            Assert.Equal(company.Name, result.Value.Name);
        }

        [Fact]
        public async Task GetCompanyAsync_ReturnFailure_WhenCompanyIdIsEmpty()
        {
            // Arrange
            var companyId = Guid.Empty;

            // Act
            var result = await _companyService.GetCompanyAsync(companyId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Company ID is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task UpdateCompanyAsync_ReturnSuccess_WhenValidRequestAndUserIsOwner()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var request = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "Updated Company Name",
                Description = "Updated description",
                Logo = "updated-logo.png"
            };

            var existingCompany = new Company
            {
                Id = companyId,
                OwnerID = ownerId,
                Name = "Original Company Name",
                Description = "Original description",
                Logo = "original-logo.png",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null
            };

            _mockCompanyRepository.Setup(r => r.GetByIdAsync(companyId)).ReturnsAsync(existingCompany);
            _mockCompanyRepository.Setup(r => r.FindByNameAsync(request.Name.Trim())).ReturnsAsync((Company?)null);
            _mockCompanyRepository.Setup(r => r.UpdateAsync(It.IsAny<Company>())).ReturnsAsync((Company c) => c);

            // Act
            var result = await _companyService.UpdateMyCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(request.Name, result.Value.Name);
            Assert.Equal(request.Description, result.Value.Description);
            Assert.Equal(request.Logo, result.Value.Logo);
        }

        [Fact]
        public async Task UpdateCompanyAsync_ReturnFailure_WhenNewNameWithWhitespaceAlreadyExists()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var otherCompanyId = Guid.NewGuid();
            
            var request = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "  Existing Company  ", // Name with whitespace that matches another company after trim
                Description = "Updated description",
                Logo = "updated-logo.png"
            };

            var existingCompany = new Company
            {
                Id = companyId,
                OwnerID = ownerId,
                Name = "Original Company Name",
                Description = "Original description",
                Logo = "original-logo.png",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null
            };

            var otherCompany = new Company
            {
                Id = otherCompanyId,
                OwnerID = Guid.NewGuid(),
                Name = "Existing Company", // This company already has the trimmed name
                Description = "Other description",
                Logo = "other-logo.png",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = null
            };

            _mockCompanyRepository.Setup(r => r.GetByIdAsync(companyId)).ReturnsAsync(existingCompany);
            _mockCompanyRepository.Setup(r => r.FindByNameAsync("Existing Company")).ReturnsAsync(otherCompany);

            // Act
            var result = await _companyService.UpdateMyCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Company name already exists.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task UpdateCompanyAsync_TrimWhitespaceFromCompanyName_WhenUpdating()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var request = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "  Updated Company Name  ", // Name with leading and trailing whitespace
                Description = "Updated description",
                Logo = "updated-logo.png"
            };

            var existingCompany = new Company
            {
                Id = companyId,
                OwnerID = ownerId,
                Name = "Original Company Name",
                Description = "Original description",
                Logo = "original-logo.png",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null
            };

            _mockCompanyRepository.Setup(r => r.GetByIdAsync(companyId)).ReturnsAsync(existingCompany);
            _mockCompanyRepository.Setup(r => r.FindByNameAsync("Updated Company Name")).ReturnsAsync((Company?)null);
            _mockCompanyRepository.Setup(r => r.UpdateAsync(It.Is<Company>(c => c.Name == "Updated Company Name"))).ReturnsAsync((Company c) => c);

            // Act
            var result = await _companyService.UpdateMyCompanyAsync(request, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Updated Company Name", result.Value.Name); // Should be trimmed
            _mockCompanyRepository.Verify(r => r.UpdateAsync(It.Is<Company>(c => c.Name == "Updated Company Name")), Times.Once);
        }

        [Fact]
        public async Task UpdateCompanyAsync_ReturnFailure_WhenUserIsNotOwner()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var differentOwnerId = Guid.NewGuid();
            var request = new UpdateCompanyRequest
            {
                Id = companyId,
                Name = "Updated Company Name",
                Description = "Updated description",
                Logo = "updated-logo.png"
            };

            var existingCompany = new Company
            {
                Id = companyId,
                OwnerID = ownerId, // Actual owner is different
                Name = "Original Company Name",
                Description = "Original description",
                Logo = "original-logo.png",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null
            };

            _mockCompanyRepository.Setup(r => r.GetByIdAsync(companyId)).ReturnsAsync(existingCompany);

            // Act
            var result = await _companyService.UpdateMyCompanyAsync(request, differentOwnerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("You are not authorized to update this company.", result.Error);
            Assert.Equal(StatusCodes.Status403Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task UpdateMyCompanyAsync_ReturnFailure_WhenOwnerIdIsEmpty()
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Updated Company Name",
                Description = "Updated description",
                Logo = "updated-logo.png"
            };

            // Act
            var result = await _companyService.UpdateMyCompanyAsync(request, Guid.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Owner ID is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task DeleteCompanyAsync_ReturnSuccess_WhenValidRequestAndUserIsOwner()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();

            var existingCompany = new Company
            {
                Id = companyId,
                OwnerID = ownerId,
                Name = "Company to Delete",
                Description = "This company will be deleted",
                Logo = "logo.png",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null
            };

            _mockCompanyRepository.Setup(r => r.GetByIdAsync(companyId)).ReturnsAsync(existingCompany);
            _mockCompanyRepository.Setup(r => r.DeleteAsync(existingCompany)).Returns(Task.CompletedTask);

            // Act
            var result = await _companyService.DeleteCompanyAsync(companyId, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(existingCompany.Id, result.Value.Id);
            Assert.Equal(existingCompany.Name, result.Value.Name);
            _mockCompanyRepository.Verify(r => r.DeleteAsync(existingCompany), Times.Once);
        }

        [Fact]
        public async Task DeleteCompanyAsync_ReturnFailure_WhenUserIsNotOwner()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var differentOwnerId = Guid.NewGuid();

            var existingCompany = new Company
            {
                Id = companyId,
                OwnerID = ownerId, // Actual owner
                Name = "Company to Delete",
                Description = "This company will be deleted",
                Logo = "logo.png",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = null
            };

            _mockCompanyRepository.Setup(r => r.GetByIdAsync(companyId)).ReturnsAsync(existingCompany);

            // Act
            var result = await _companyService.DeleteCompanyAsync(companyId, differentOwnerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("You are not authorized to delete this company.", result.Error);
            Assert.Equal(StatusCodes.Status403Forbidden, result.ErrorCode);
            _mockCompanyRepository.Verify(r => r.DeleteAsync(It.IsAny<Company>()), Times.Never);
        }
    }
}