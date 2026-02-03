using Domain.Models.Requests;
using Microsoft.AspNetCore.Http;

namespace API.Tests
{
    public class CreateCategoryNodeRequestValidationShould
    {
        [Fact]
        public void ReturnSuccess_WhenCategoryNodeWithExtraAttributesIsValid()
        {
            // Arrange
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                SortOrder = 1,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "SKU",
                        Name_fr = "SKU",
                        AttributeType = "string",
                        SortOrder = 1
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ReturnFailure_WhenExtraAttributesProvidedForDepartementNode()
        {
            // Arrange
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Electronics",
                Name_fr = "Électronique",
                NodeType = "Departement",
                ParentId = null,
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "SKU",
                        Name_fr = "SKU"
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CategoryMandatoryExtraAttributes can only be provided when creating a Category node.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenExtraAttributesProvidedForNavigationNode()
        {
            // Arrange
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Audio",
                Name_fr = "Audio",
                NodeType = "Navigation",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "SKU",
                        Name_fr = "SKU"
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CategoryMandatoryExtraAttributes can only be provided when creating a Category node.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenExtraAttributeEnglishNameIsEmpty()
        {
            // Arrange
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "",
                        Name_fr = "SKU"
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CategoryMandatoryExtraAttribute English name is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenExtraAttributeFrenchNameIsEmpty()
        {
            // Arrange
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "SKU",
                        Name_fr = ""
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CategoryMandatoryExtraAttribute French name is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenExtraAttributeEnglishNameExceedsMaxLength()
        {
            // Arrange
            var longName = new string('a', 101); // 101 characters
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = longName,
                        Name_fr = "SKU"
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CategoryMandatoryExtraAttribute English name cannot exceed 100 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenExtraAttributeFrenchNameExceedsMaxLength()
        {
            // Arrange
            var longName = new string('a', 101); // 101 characters
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "SKU",
                        Name_fr = longName
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CategoryMandatoryExtraAttribute French name cannot exceed 100 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnFailure_WhenExtraAttributeTypeExceedsMaxLength()
        {
            // Arrange
            var longType = new string('a', 51); // 51 characters
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "SKU",
                        Name_fr = "SKU",
                        AttributeType = longType
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CategoryMandatoryExtraAttribute AttributeType cannot exceed 50 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void ReturnSuccess_WhenMultipleExtraAttributesAreValid()
        {
            // Arrange
            var request = new CreateCategoryNodeRequest
            {
                Name_en = "Speakers",
                Name_fr = "Haut-parleurs",
                NodeType = "Category",
                ParentId = Guid.NewGuid(),
                IsActive = true,
                CategoryMandatoryExtraAttributes = new List<CreateCategoryMandatoryExtraAttributeDto>
                {
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "SKU",
                        Name_fr = "SKU",
                        AttributeType = "string",
                        SortOrder = 1
                    },
                    new CreateCategoryMandatoryExtraAttributeDto
                    {
                        Name_en = "Dimensions",
                        Name_fr = "Dimensions",
                        AttributeType = "string",
                        SortOrder = 2
                    }
                }
            };

            // Act
            var result = request.Validate();

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
