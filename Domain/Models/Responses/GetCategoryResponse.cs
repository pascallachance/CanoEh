namespace Domain.Models.Responses
{
    public class GetCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public List<GetCategoryResponse> Subcategories { get; set; } = new();
    }
}