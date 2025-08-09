namespace Infrastructure.Data
{
    public class TaxRate
    {
        public Guid ID { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public required string Country { get; set; }
        public string? ProvinceState { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}