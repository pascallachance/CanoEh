namespace Domain.Models.Responses
{
    public class GetItemReviewResponse
    {
        public Guid Id { get; set; }
        public Guid ItemID { get; set; }
        public Guid UserID { get; set; }
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
