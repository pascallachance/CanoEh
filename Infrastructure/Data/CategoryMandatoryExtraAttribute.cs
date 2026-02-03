namespace Infrastructure.Data
{
    /// <summary>
    /// Represents a mandatory extra attribute for a CategoryNode.
    /// These extra attributes are required when creating or editing item variants in a category.
    /// They differ from CategoryMandatoryAttribute in that they are used for variant-level details
    /// such as SKU, Dimensions, etc.
    /// </summary>
    public class CategoryMandatoryExtraAttribute
    {
        public Guid Id { get; set; }
        public Guid CategoryNodeId { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string? AttributeType { get; set; }
        public int? SortOrder { get; set; }
    }
}
