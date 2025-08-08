namespace Infrastructure.Data
{
    public class Address
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public string? State { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public required string AddressType { get; set; } // 'Delivery', 'Billing', 'Company'
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}