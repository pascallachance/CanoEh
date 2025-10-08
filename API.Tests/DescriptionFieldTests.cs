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
            var description_en = "This is a test item description in English with special characters: !@#$%^&*()";
            var description_fr = "Ceci est une description d'article de test en français avec des caractères spéciaux : !@#$%^&*()";
            
            // Act
            var item = new Item
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = description_en,
                Description_fr = description_fr,
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            // Assert
            Assert.Equal(description_en, item.Description_en);
            Assert.Equal(description_fr, item.Description_fr);
            Assert.True(item.Description_en?.Length > 0);
            Assert.True(item.Description_fr?.Length > 0);
        }

        [Fact]
        public void Item_Description_ShouldAcceptNullValue()
        {
            // Arrange & Act
            var item = new Item
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = null, // Should be allowed
                Description_fr = null, // Should be allowed
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            // Assert
            Assert.Null(item.Description_en);
            Assert.Null(item.Description_fr);
        }

        [Fact]
        public void CreateItemRequest_Description_ShouldAcceptStringValue()
        {
            // Arrange
            var description_en = "Test description for create request in English";
            var description_fr = "Description de test pour la demande de création en français";
            
            // Act
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = description_en,
                Description_fr = description_fr,
                CategoryID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>(),
                ItemAttributes = new List<CreateItemAttributeRequest>()
            };

            // Assert
            Assert.Equal(description_en, request.Description_en);
            Assert.Equal(description_fr, request.Description_fr);
        }

        [Fact]
        public void UpdateItemRequest_Description_ShouldAcceptStringValue()
        {
            // Arrange
            var description_en = "Updated description for item in English";
            var description_fr = "Description mise à jour pour l'article en français";
            
            // Act
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = description_en,
                Description_fr = description_fr,
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>()
            };

            // Assert
            Assert.Equal(description_en, request.Description_en);
            Assert.Equal(description_fr, request.Description_fr);
        }

        [Fact]
        public void GetItemResponse_Description_ShouldAcceptStringValue()
        {
            // Arrange
            var description_en = "Test description for response in English";
            var description_fr = "Description de test pour la réponse en français";
            
            // Act
            var response = new GetItemResponse
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Description_en = description_en,
                Description_fr = description_fr,
                CategoryID = Guid.NewGuid(),
                Variants = new List<ItemVariant>(),
                ItemAttributes = new List<ItemAttribute>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                Deleted = false
            };

            // Assert
            Assert.Equal(description_en, response.Description_en);
            Assert.Equal(description_fr, response.Description_fr);
        }
    }
}