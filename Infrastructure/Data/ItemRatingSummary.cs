namespace Infrastructure.Data
{
    public class ItemRatingSummary
    {
        public Guid ItemID { get; set; }
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}
