namespace Domain.Models.Responses
{
    /// <summary>
    /// Data transfer object for item attributes in API responses.
    /// Excludes the ItemID foreign key since it's redundant when nested under an Item.
    /// </summary>
    public class ItemAttributeDto
    {
        public Guid Id { get; set; }
        public string AttributeName_en { get; set; } = string.Empty;
        public string? AttributeName_fr { get; set; }
        public string Attributes_en { get; set; } = string.Empty;
        public string? Attributes_fr { get; set; }
    }
}
