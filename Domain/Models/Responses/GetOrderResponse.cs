namespace Domain.Models.Responses
{
    public class GetOrderResponse
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusName_en { get; set; } = string.Empty;
        public string StatusName_fr { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderItemResponse> OrderItems { get; set; } = new();
        public List<OrderAddressResponse> Addresses { get; set; } = new();
        public OrderPaymentResponse? Payment { get; set; }
    }
}