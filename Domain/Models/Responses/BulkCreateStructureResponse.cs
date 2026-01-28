namespace Domain.Models.Responses
{
    public class CategoryNodeResponseDto
    {
        public Guid Id { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public string NodeType { get; set; } = "Category";
        public bool IsActive { get; set; }
        public int? SortOrder { get; set; }
        public List<CategoryMandatoryAttributeResponseDto>? CategoryMandatoryAttributes { get; set; }
    }

    public class NavigationNodeResponseDto
    {
        public Guid Id { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public string NodeType { get; set; } = "Navigation";
        public bool IsActive { get; set; }
        public int? SortOrder { get; set; }
        public List<NavigationNodeResponseDto>? NavigationNodes { get; set; }
        public List<CategoryNodeResponseDto>? CategoryNodes { get; set; }
    }

    public class DepartementNodeResponseDto
    {
        public Guid Id { get; set; }
        public required string Name_en { get; set; }
        public required string Name_fr { get; set; }
        public string NodeType { get; set; } = "Departement";
        public bool IsActive { get; set; }
        public int? SortOrder { get; set; }
        public List<NavigationNodeResponseDto>? NavigationNodes { get; set; }
        public List<CategoryNodeResponseDto>? CategoryNodes { get; set; }
    }

    public class BulkCreateStructureResponse
    {
        public List<DepartementNodeResponseDto> Departements { get; set; } = new();
        public int TotalNodesCreated { get; set; }
    }
}
