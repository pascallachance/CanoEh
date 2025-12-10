namespace Infrastructure.Data
{
    public class Item
    {
        public Guid Id { get; set; }
        public Guid SellerID { get; set; } // Seller will be the user that created the Item (Currently Seller are not implemented)
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string? Description_en { get; set; }
        public string? Description_fr { get; set; }
        public string? ImageUrl { get; set; }
        public Guid CategoryID { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<ItemAttribute> ItemAttributes { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool Deleted { get; set; } = false;
    }
}