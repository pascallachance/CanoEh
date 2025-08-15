namespace Domain.Models.Responses
{
    public class CreateOrderResponse
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

    public class OrderItemResponse
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
        public string Status { get; set; } = string.Empty;
        public DateTime? DeliveredAt { get; set; }
        public string? OnHoldReason { get; set; }
    }

    public class OrderAddressResponse
    {
        public Guid ID { get; set; }
        public Guid OrderID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? ProvinceState { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class OrderPaymentResponse
    {
        public Guid ID { get; set; }
        public Guid OrderID { get; set; }
        public Guid? PaymentMethodID { get; set; }
        public decimal Amount { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? ProviderReference { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}