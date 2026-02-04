namespace Domain.Models.Responses
{
    /// <summary>
    /// Data transfer object for item variants in API responses.
    /// Excludes the ItemId foreign key since it's redundant when nested under an Item.
    /// </summary>
    public class ItemVariantDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? ProductIdentifierType { get; set; }
        public string? ProductIdentifierValue { get; set; }
        public string? ImageUrls { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? ItemVariantName_en { get; set; }
        public string? ItemVariantName_fr { get; set; }
        public List<ItemVariantAttributeDto> ItemVariantAttributes { get; set; } = [];
        public List<ItemVariantFeaturesDto> ItemVariantFeatures { get; set; } = [];
        public bool Deleted { get; set; }
        
        // Offer fields
        public decimal? Offer { get; set; }
        public DateTime? OfferStart { get; set; }
        public DateTime? OfferEnd { get; set; }
    }
}
