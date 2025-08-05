namespace Infrastructure.Data
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; } // null for root categories
        public Category? ParentCategory { get; set; }
        public List<Category> Subcategories { get; set; } = new();
        public List<Item> Items { get; set; } = new();
    }
}