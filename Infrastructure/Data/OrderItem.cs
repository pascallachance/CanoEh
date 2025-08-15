namespace Infrastructure.Data
{
    public class OrderItem
    {
        public Guid ID { get; set; }
        public Guid OrderID { get; set; }
        public Guid ItemID { get; set; }
        public Guid ItemVariantID { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string? VariantName_en { get; set; }
        public string? VariantName_fr { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? DeliveredAt { get; set; }
        public string? OnHoldReason { get; set; }
    }
}