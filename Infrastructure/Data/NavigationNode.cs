namespace Infrastructure.Data
{
    /// <summary>
    /// Navigation node used to group other NavigationNodes or CategoryNodes.
    /// ParentId points to a DepartementNode or another NavigationNode.
    /// </summary>
    public class NavigationNode : BaseNode
    {
        public NavigationNode()
        {
            NodeType = "Navigation";
        }
    }
}
