using Infrastructure.Data;
using Domain.Models.Requests;
using Domain.Models.Responses;

namespace API.Tests
{
    public class DescriptionFieldTests
    {
        [Fact]
        public void Item_Description_ShouldAcceptStringValue()
        {
            // Arrange
            var description = "This is a test item description with special characters: !@#$%^&*()";
            
            // Act
            var item = new Item
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name = "Test Item",
                Description = description,
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            // Assert
            Assert.Equal(description, item.Description);
            Assert.True(item.Description?.Length > 0);
        }

        [Fact]
        public void Item_Description_ShouldAcceptNullValue()
        {
            // Arrange & Act
            var item = new Item
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name = "Test Item",
                Description = null, // Should be allowed
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            // Assert
            Assert.Null(item.Description);
        }

        [Fact]
        public void CreateItemRequest_Description_ShouldAcceptStringValue()
        {
            // Arrange
            var description = "Test description for create request";
            
            // Act
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name = "Test Item",
                Description = description,
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>()
            };

            // Assert
            Assert.Equal(description, request.Description);
        }

        [Fact]
        public void UpdateItemRequest_Description_ShouldAcceptStringValue()
        {
            // Arrange
            var description = "Updated description for item";
            
            // Act
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name = "Test Item",
                Description = description,
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>()
            };

            // Assert
            Assert.Equal(description, request.Description);
        }

        [Fact]
        public void GetItemResponse_Description_ShouldAcceptStringValue()
        {
            // Arrange
            var description = "Test description for response";
            
            // Act
            var response = new GetItemResponse
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name = "Test Item",
                Description = description,
                Brand = "Test Brand",
                Category = "Test Category",
                Variants = new List<ItemVariant>(),
                ImageUrls = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            // Assert
            Assert.Equal(description, response.Description);
        }
    }
}