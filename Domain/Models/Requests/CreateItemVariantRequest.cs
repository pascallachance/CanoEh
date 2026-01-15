using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Requests
{
    public class CreateItemVariantRequest
    {
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? ProductIdentifierType { get; set; }
        public string? ProductIdentifierValue { get; set; }
        public string? ImageUrls { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? ItemVariantName_en { get; set; }
        public string? ItemVariantName_fr { get; set; }
        public List<CreateItemVariantAttributeRequest> ItemVariantAttributes { get; set; } = new();
        public bool Deleted { get; set; } = false;
        
        // Offer fields
        [Range(0, 100, ErrorMessage = "Offer must be between 0 and 100")]
        public decimal? Offer { get; set; }
        public DateTime? OfferStart { get; set; }
        public DateTime? OfferEnd { get; set; }
    }
}
