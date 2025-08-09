namespace Domain.Models.Responses
{
    public class UpdateTaxRateResponse
    {
        public Guid ID { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? ProvinceState { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}