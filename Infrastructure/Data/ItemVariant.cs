namespace Infrastructure.Data
{
    public class ItemVariant
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; } // Id of related item for DB relation
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? ProductIdentifierType { get; set; } // e.g., 'UPC', 'GTIN', etc.
        public string? ProductIdentifierValue { get; set; }
        public string? ImageUrls { get; set; } // Comma-separated URLs
        public string? ThumbnailUrl { get; set; }
        public string? ItemVariantName_en { get; set; }
        public string? ItemVariantName_fr { get; set; }
        public List<ItemVariantAttribute> ItemVariantAttributes { get; set; } = [];
        public List<ItemAttribute> ItemAttributes { get; set; } = [];
        public bool Deleted { get; set; } = false;
        
        // Offer fields
        public decimal? Offer { get; set; } // Percentage (0-100)
        public DateTime? OfferStart { get; set; }
        public DateTime? OfferEnd { get; set; }
    }
}