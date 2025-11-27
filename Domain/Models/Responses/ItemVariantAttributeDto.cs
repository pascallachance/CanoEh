namespace Domain.Models.Responses
{
    /// <summary>
    /// Data transfer object for item variant attributes in API responses.
    /// Excludes the ItemVariantID foreign key since it's redundant when nested under an ItemVariant.
    /// </summary>
    public class ItemVariantAttributeDto
    {
        public Guid Id { get; set; }
        public string AttributeName_en { get; set; } = string.Empty;
        public string? AttributeName_fr { get; set; }
        public string Attributes_en { get; set; } = string.Empty;
        public string? Attributes_fr { get; set; }
    }
}
