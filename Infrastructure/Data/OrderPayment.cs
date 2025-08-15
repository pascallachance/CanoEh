namespace Infrastructure.Data
{
    public class OrderPayment
    {
        public Guid ID { get; set; }
        public Guid OrderID { get; set; }
        public Guid? PaymentMethodID { get; set; }
        public decimal Amount { get; set; }
        public string Provider { get; set; } = string.Empty; // e.g., "Stripe", "PayPal"
        public string? ProviderReference { get; set; } // e.g., charge ID
        public DateTime? PaidAt { get; set; }
    }
}