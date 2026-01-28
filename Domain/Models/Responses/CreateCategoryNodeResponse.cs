namespace Domain.Models.Responses
{
    public class CreateCategoryNodeResponse
    {
        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; }
        public int? SortOrder { get; set; }
    }
}
