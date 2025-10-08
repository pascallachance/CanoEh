namespace Domain.Models.Requests
{
    public class CreateItemVariantRequest
    {
        public string Id { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? ProductIdentifierType { get; set; }
        public string? ProductIdentifierValue { get; set; }
        public string? ImageUrls { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? ItemVariantName_en { get; set; }
        public string? ItemVariantName_fr { get; set; }
        public List<object> ItemVariantAttributes { get; set; } = new();
        public bool Deleted { get; set; } = false;
    }
}
