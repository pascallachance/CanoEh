namespace Domain.Models.Requests
{
    public class BatchUpdateItemVariantOffersRequest
    {
        public List<UpdateItemVariantOfferRequest> OfferUpdates { get; set; } = new();
    }
}
