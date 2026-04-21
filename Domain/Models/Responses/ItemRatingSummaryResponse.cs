namespace Domain.Models.Responses
{
    public class ItemRatingSummaryResponse
    {
        public Guid ItemID { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
