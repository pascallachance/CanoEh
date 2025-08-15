namespace Infrastructure.Data
{
    public class OrderAddress
    {
        public Guid ID { get; set; }
        public Guid OrderID { get; set; }
        public string Type { get; set; } = string.Empty; // "Shipping" or "Billing"
        public string FullName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? ProvinceState { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}