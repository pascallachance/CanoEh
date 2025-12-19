using Domain.Models.Requests;
using Microsoft.AspNetCore.Http;

namespace API.Tests
{
    public class CompanyFieldValidationShould
    {
        [Fact]
        public void CreateCompanyRequest_ReturnSuccess_WhenAllValidationsPassed()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
                Description = "A test company",
                Logo = "logo.png",
                IdentityDocumentType = "passport",
                CompanyType = "private company"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData("passport")]
        [InlineData("Driver Licence")]
        [InlineData("government delivered document")]
        public void CreateCompanyRequest_ReturnSuccess_WhenIdentityDocumentTypeIsValid(string identityDocumentType)
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
                IdentityDocumentType = identityDocumentType
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateCompanyRequest_ReturnFailure_WhenIdentityDocumentTypeIsInvalid()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
                IdentityDocumentType = "invalid document type"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("IdentityDocumentType must be 'passport', 'Driver Licence', or 'government delivered document'.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Theory]
        [InlineData("public company")]
        [InlineData("listed company")]
        [InlineData("private company")]
        [InlineData("charity organization")]
        [InlineData("particular")]
        public void CreateCompanyRequest_ReturnSuccess_WhenCompanyTypeIsValid(string companyType)
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
                CompanyType = companyType
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateCompanyRequest_ReturnFailure_WhenCompanyTypeIsInvalid()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
                CompanyType = "invalid company type"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CompanyType must be 'public company', 'listed company', 'private company', 'charity organization', or 'particular'.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreateCompanyRequest_ReturnSuccess_WhenOptionalFieldsAreNull()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "test@company.com",
                Description = null,
                Logo = null,
                CountryOfCitizenship = null,
                FullBirthName = null,
                CountryOfBirth = null,
                BirthDate = null,
                IdentityDocumentType = null,
                IdentityDocument = null,
                BankDocument = null,
                FacturationDocument = null,
                CompanyPhone = null,
                CompanyType = null,
                Address1 = null,
                Address2 = null,
                Address3 = null,
                City = null,
                ProvinceState = null,
                Country = null,
                PostalCode = null
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateCompanyRequest_ReturnSuccess_WhenAllValidationsPassed()
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Email = "test@company.com",
                Description = "A test company",
                Logo = "logo.png",
                IdentityDocumentType = "passport",
                CompanyType = "private company"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData("passport")]
        [InlineData("Driver Licence")]
        [InlineData("government delivered document")]
        public void UpdateCompanyRequest_ReturnSuccess_WhenIdentityDocumentTypeIsValid(string identityDocumentType)
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Email = "test@company.com",
                IdentityDocumentType = identityDocumentType
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateCompanyRequest_ReturnFailure_WhenIdentityDocumentTypeIsInvalid()
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Email = "test@company.com",
                IdentityDocumentType = "invalid document type"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("IdentityDocumentType must be 'passport', 'Driver Licence', or 'government delivered document'.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Theory]
        [InlineData("public company")]
        [InlineData("listed company")]
        [InlineData("private company")]
        [InlineData("charity organization")]
        [InlineData("particular")]
        public void UpdateCompanyRequest_ReturnSuccess_WhenCompanyTypeIsValid(string companyType)
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Email = "test@company.com",
                CompanyType = companyType
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateCompanyRequest_ReturnFailure_WhenCompanyTypeIsInvalid()
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Email = "test@company.com",
                CompanyType = "invalid company type"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CompanyType must be 'public company', 'listed company', 'private company', 'charity organization', or 'particular'.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreateCompanyRequest_ReturnFailure_WhenEmailIsEmpty()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = ""
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreateCompanyRequest_ReturnFailure_WhenEmailIsInvalid()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = "not-an-email"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is not valid.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Theory]
        [InlineData("test@company.com")]
        [InlineData("user.name@example.co.uk")]
        [InlineData("test+tag@domain.com")]
        public void CreateCompanyRequest_ReturnSuccess_WhenEmailIsValid(string email)
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Email = email
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateCompanyRequest_ReturnFailure_WhenEmailIsEmpty()
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Email = ""
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateCompanyRequest_ReturnFailure_WhenEmailIsInvalid()
        {
            // Arrange
            var request = new UpdateCompanyRequest
            {
                Id = Guid.NewGuid(),
                Name = "Test Company",
                Email = "invalid-email"
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Email is not valid.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }
    }
}