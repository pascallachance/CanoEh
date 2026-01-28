namespace Infrastructure.Data
{
    /// <summary>
    /// Base class for product hierarchy nodes (Departement, Navigation, Category)
    /// </summary>
    public abstract class BaseNode
    {
        public const string NodeTypeDepartement = "Departement";
        public const string NodeTypeNavigation = "Navigation";
        public const string NodeTypeCategory = "Category";
        public const int MaxNameLength = 200;

        public Guid Id { get; set; }
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
        public string NodeType { get; protected set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
