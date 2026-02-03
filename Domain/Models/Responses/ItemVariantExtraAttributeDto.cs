namespace Domain.Models.Responses
{
    /// <summary>
    /// Data transfer object for item variant extra attributes in API responses.
    /// Excludes the ItemVariantId foreign key since it's redundant when nested under an ItemVariant.
    /// </summary>
    public class ItemVariantExtraAttributeDto
    {
        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string? Name_fr { get; set; }
        public string? Value_en { get; set; }
        public string? Value_fr { get; set; }
    }
}
