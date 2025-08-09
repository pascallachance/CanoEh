using Infrastructure.Data;

namespace Domain.Models.Responses
{
    public class UpdateItemResponse
    {
        public Guid Id { get; set; }
        public Guid SellerID { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public required string Name_fr { get; set; }
        public string? Description_en { get; set; }
        public string? Description_fr { get; set; }
        public Guid CategoryID { get; set; }
        public List<ItemVariant> Variants { get; set; } = new();
        public List<ItemAttribute> ItemAttributes { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }
}