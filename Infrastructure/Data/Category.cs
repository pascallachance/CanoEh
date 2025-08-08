namespace Infrastructure.Data
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; } // null for root categories
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}