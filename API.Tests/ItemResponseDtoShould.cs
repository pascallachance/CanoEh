using Domain.Models.Responses;

namespace API.Tests
{
    /// <summary>
    /// Tests to verify that the Item response DTOs correctly exclude redundant foreign key IDs
    /// that are unnecessary when nested under their parent entities.
    /// </summary>
    public class ItemResponseDtoShould
    {
        [Fact]
        public void ItemVariantDto_NotExposeItemId_Property()
        {
            // Verify ItemVariantDto does not have an ItemId property
            // since it's redundant when nested under an Item
            var variantType = typeof(ItemVariantDto);
            var itemIdProperty = variantType.GetProperty("ItemId");
            
            Assert.Null(itemIdProperty);
        }

        [Fact]
        public void ItemVariantAttributeDto_NotExposeItemVariantID_Property()
        {
            // Verify ItemVariantAttributeDto does not have an ItemVariantID property
            // since it's redundant when nested under an ItemVariant
            var variantAttrType = typeof(ItemVariantAttributeDto);
            var itemVariantIdProperty = variantAttrType.GetProperty("ItemVariantID");
            
            Assert.Null(itemVariantIdProperty);
        }

        [Fact]
        public void ItemAttributeDto_NotExposeItemID_Property()
        {
            // Verify ItemAttributeDto does not have an ItemID property
            // since it's redundant when nested under an Item
            var itemAttrType = typeof(ItemAttributeDto);
            var itemIdProperty = itemAttrType.GetProperty("ItemID");
            
            Assert.Null(itemIdProperty);
        }

        [Fact]
        public void ItemVariantDto_HaveExpectedProperties()
        {
            // Verify ItemVariantDto has all expected properties except the excluded ItemId
            var variantType = typeof(ItemVariantDto);
            
            Assert.NotNull(variantType.GetProperty("Id"));
            Assert.NotNull(variantType.GetProperty("Price"));
            Assert.NotNull(variantType.GetProperty("StockQuantity"));
            Assert.NotNull(variantType.GetProperty("Sku"));
            Assert.NotNull(variantType.GetProperty("ProductIdentifierType"));
            Assert.NotNull(variantType.GetProperty("ProductIdentifierValue"));
            Assert.NotNull(variantType.GetProperty("ImageUrls"));
            Assert.NotNull(variantType.GetProperty("ThumbnailUrl"));
            Assert.NotNull(variantType.GetProperty("ItemVariantName_en"));
            Assert.NotNull(variantType.GetProperty("ItemVariantName_fr"));
            Assert.NotNull(variantType.GetProperty("ItemVariantAttributes"));
            Assert.NotNull(variantType.GetProperty("Deleted"));
        }

        [Fact]
        public void ItemVariantAttributeDto_HaveExpectedProperties()
        {
            // Verify ItemVariantAttributeDto has all expected properties except the excluded ItemVariantID
            var variantAttrType = typeof(ItemVariantAttributeDto);
            
            Assert.NotNull(variantAttrType.GetProperty("Id"));
            Assert.NotNull(variantAttrType.GetProperty("AttributeName_en"));
            Assert.NotNull(variantAttrType.GetProperty("AttributeName_fr"));
            Assert.NotNull(variantAttrType.GetProperty("Attributes_en"));
            Assert.NotNull(variantAttrType.GetProperty("Attributes_fr"));
        }

        [Fact]
        public void ItemAttributeDto_HaveExpectedProperties()
        {
            // Verify ItemAttributeDto has all expected properties except the excluded ItemID
            var itemAttrType = typeof(ItemAttributeDto);
            
            Assert.NotNull(itemAttrType.GetProperty("Id"));
            Assert.NotNull(itemAttrType.GetProperty("AttributeName_en"));
            Assert.NotNull(itemAttrType.GetProperty("AttributeName_fr"));
            Assert.NotNull(itemAttrType.GetProperty("Attributes_en"));
            Assert.NotNull(itemAttrType.GetProperty("Attributes_fr"));
        }
    }
}
