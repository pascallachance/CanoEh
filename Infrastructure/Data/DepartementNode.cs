namespace Infrastructure.Data
{
    /// <summary>
    /// Root level node in the product hierarchy. ParentId should always be NULL.
    /// </summary>
    public class DepartementNode : BaseNode
    {
        public DepartementNode()
        {
            NodeType = NodeTypeDepartement;
        }
    }
}
