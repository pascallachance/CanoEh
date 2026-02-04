namespace Domain.Models.Responses
{
    public class CategoryMandatoryAttributeResponseDto
    {
        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string? AttributeType { get; set; }
        public int? SortOrder { get; set; }
    }

    public class CreateCategoryNodeResponse
    {
        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; }
        public int? SortOrder { get; set; }
        public List<CategoryMandatoryAttributeResponseDto> CategoryMandatoryAttributes { get; set; } = new();
    }
}
