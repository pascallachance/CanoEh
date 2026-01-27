namespace Infrastructure.Data
{
    /// <summary>
    /// Category node representing product categories that can be assigned to Products.
    /// ParentId points to a NavigationNode or DepartementNode.
    /// </summary>
    public class CategoryNode : BaseNode
    {
        public CategoryNode()
        {
            NodeType = "Category";
        }
    }
}
