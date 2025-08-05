using Infrastructure.Data;

namespace Domain.Models.Responses
{
    public class UpdateItemResponse
    {
        public Guid Id { get; set; }
        public Guid SellerID { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? DescriptionID { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }
}