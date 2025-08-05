namespace Infrastructure.Data
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; } // null for root categories
        public Category? ParentCategory { get; set; }
        public List<Category> Subcategories { get; set; } = new();
        public List<Item> Items { get; set; } = new();
    }
}