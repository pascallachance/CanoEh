namespace Domain.Models.Responses
{
    public class GetItemResponse
    {
        public Guid Id { get; set; }
        public Guid SellerID { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string? Description_en { get; set; }
        public string? Description_fr { get; set; }
        public string? ImageUrl { get; set; }
        public Guid CategoryID { get; set; }
        public List<ItemVariantDto> Variants { get; set; } = new();
        public List<ItemVariantFeaturesDto> ItemVariantFeatures { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }
}