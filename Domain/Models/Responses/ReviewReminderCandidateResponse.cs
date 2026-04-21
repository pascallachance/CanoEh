namespace Domain.Models.Responses
{
    public class ReviewReminderCandidateResponse
    {
        public Guid UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public Guid ItemID { get; set; }
        public string ItemName_en { get; set; } = string.Empty;
        public string ItemName_fr { get; set; } = string.Empty;
        public DateTime DeliveredAt { get; set; }
    }
}
