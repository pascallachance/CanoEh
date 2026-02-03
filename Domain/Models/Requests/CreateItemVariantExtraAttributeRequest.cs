namespace Domain.Models.Requests
{
    public class CreateItemVariantExtraAttributeRequest
    {
        public string Name_en { get; set; } = string.Empty;
        public string? Name_fr { get; set; }
        public string? Value_en { get; set; }
        public string? Value_fr { get; set; }
    }
}
