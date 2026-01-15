using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Requests
{
    public class UpdateItemVariantOfferRequest
    {
        public Guid VariantId { get; set; }
        
        [Range(0, 100, ErrorMessage = "Offer must be between 0 and 100")]
        public decimal? Offer { get; set; }
        
        public DateTime? OfferStart { get; set; }
        public DateTime? OfferEnd { get; set; }
    }
}
