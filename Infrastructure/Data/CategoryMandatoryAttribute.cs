namespace Infrastructure.Data
{
    /// <summary>
    /// Represents a mandatory attribute for a CategoryNode.
    /// These attributes are required when creating or editing products in a category.
    /// </summary>
    public class CategoryMandatoryAttribute
    {
        public Guid Id { get; set; }
        public Guid CategoryNodeId { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string? AttributeType { get; set; }
        public int? SortOrder { get; set; }
    }
}
