namespace Infrastructure.Data
{
    public class Item
    {
        public Guid Id { get; set; }
        public Guid SellerID { get; set; } // Seller will be the user that created the Item (Currently Seller are not implemented)
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool Deleted { get; set; } = false;
    }
}