namespace Infrastructure.Data
{
    public class ItemVariant
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; } // Id of related item for DB relation
        public Dictionary<string, string> Attributes { get; set; } = new(); // e.g. { "Color": "Red", "Size": "XL" }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Sku { get; set; }
        public List<string> ThumbnailUrls { get; set; } = new(); // Thumbnails for this variant
        public bool Deleted { get; set; } = false;
    }
}