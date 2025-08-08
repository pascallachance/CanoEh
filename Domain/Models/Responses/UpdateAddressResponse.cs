namespace Domain.Models.Responses
{
    public class UpdateAddressResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public required string City { get; set; }
        public string? ProvinceState { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }
        public required string AddressType { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}