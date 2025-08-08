namespace Domain.Models.Responses
{
    public class CreatePaymentMethodResponse
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public required string Type { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardBrand { get; set; }
        public int? ExpirationMonth { get; set; }
        public int? ExpirationYear { get; set; }
        public string? BillingAddress { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}