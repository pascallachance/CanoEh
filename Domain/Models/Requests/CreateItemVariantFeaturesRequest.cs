namespace Domain.Models.Requests
{
    public class CreateItemVariantFeaturesRequest
    {
        public string AttributeName_en { get; set; } = string.Empty;
        public string? AttributeName_fr { get; set; }
        public string Attributes_en { get; set; } = string.Empty;
        public string? Attributes_fr { get; set; }
    }
}
