namespace Infrastructure.Data
{
    public class Order
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public int StatusID { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}