using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class ProductNodeRepository(string connectionString) : GenericRepository<BaseNode>(connectionString), IProductNodeRepository
    {
        public override async Task<BaseNode> AddAsync(BaseNode entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Validate node type
            if (entity.NodeType != BaseNode.NodeTypeDepartement && entity.NodeType != BaseNode.NodeTypeNavigation && entity.NodeType != BaseNode.NodeTypeCategory)
            {
                throw new ArgumentException($"Invalid NodeType. Must be '{BaseNode.NodeTypeDepartement}', '{BaseNode.NodeTypeNavigation}', or '{BaseNode.NodeTypeCategory}'.");
            }

            // Validate ParentId constraints
            if (entity.NodeType == BaseNode.NodeTypeDepartement && entity.ParentId.HasValue)
            {
                throw new InvalidOperationException("Departement nodes cannot have a parent.");
            }

            if (entity.NodeType != BaseNode.NodeTypeDepartement && !entity.ParentId.HasValue)
            {
                throw new InvalidOperationException($"{entity.NodeType} nodes must have a parent.");
            }

            var query = @"
INSERT INTO dbo.ProductNode (Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt)
VALUES (@Id, @Name_en, @Name_fr, @NodeType, @ParentId, @IsActive, @SortOrder, @CreatedAt)";

            var parameters = new
            {
                entity.Id,
                entity.Name_en,
                entity.Name_fr,
                entity.NodeType,
                entity.ParentId,
                entity.IsActive,
                entity.SortOrder,
                entity.CreatedAt
            };

            await dbConnection.ExecuteAsync(query, parameters);

            return entity;
        }

        public override async Task<int> CountAsync(Func<BaseNode, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var nodes = await GetAllAsync();
            return nodes.Count(predicate);
        }

        public override async Task DeleteAsync(BaseNode entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Check if node has children
            var hasChildren = await HasChildrenAsync(entity.Id);
            if (hasChildren)
            {
                throw new InvalidOperationException("Cannot delete node that has children.");
            }

            // Check if category node has items
            if (entity.NodeType == "Category")
            {
                var hasItems = await HasItemsAsync(entity.Id);
                if (hasItems)
                {
                    throw new InvalidOperationException("Cannot delete category node that has items.");
                }
            }

            var query = "DELETE FROM dbo.ProductNode WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.ProductNode WHERE Id = @id", new { id });
        }

        public override async Task<IEnumerable<BaseNode>> FindAsync(Func<BaseNode, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var nodes = await GetAllAsync();
            return nodes.Where(predicate);
        }

        public override async Task<IEnumerable<BaseNode>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM dbo.ProductNode
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<ProductNodeDto>(query);
            
            return nodesData.Select(MapToNode);
        }

        public override async Task<BaseNode> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var node = await GetNodeByIdAsync(id);
            return node ?? throw new InvalidOperationException($"ProductNode with id {id} not found");
        }

        public override async Task<BaseNode> UpdateAsync(BaseNode entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Validate ParentId doesn't create a circular reference
            if (entity.ParentId.HasValue)
            {
                await ValidateNoCircularReferenceAsync(entity.Id, entity.ParentId.Value);
            }

            // Validate ParentId constraints for node types
            if (entity.NodeType == BaseNode.NodeTypeDepartement && entity.ParentId.HasValue)
            {
                throw new InvalidOperationException("Departement nodes cannot have a parent.");
            }

            if (entity.NodeType != BaseNode.NodeTypeDepartement && !entity.ParentId.HasValue)
            {
                throw new InvalidOperationException($"{entity.NodeType} nodes must have a parent.");
            }

            var query = @"
UPDATE dbo.ProductNode
SET Name_en = @Name_en,
    Name_fr = @Name_fr,
    ParentId = @ParentId,
    IsActive = @IsActive,
    SortOrder = @SortOrder,
    UpdatedAt = @UpdatedAt
WHERE Id = @Id";

            var parameters = new
            {
                entity.Id,
                entity.Name_en,
                entity.Name_fr,
                entity.ParentId,
                entity.IsActive,
                entity.SortOrder,
                entity.UpdatedAt
            };

            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        // IProductNodeRepository specific methods
        public async Task<BaseNode?> GetNodeByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM dbo.ProductNode
WHERE Id = @id";

            var dto = await dbConnection.QueryFirstOrDefaultAsync<ProductNodeDto>(query, new { id });
            
            if (dto == null)
                return null;

            return MapToNode(dto);
        }

        public async Task<IEnumerable<BaseNode>> GetRootNodesAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM dbo.ProductNode
WHERE ParentId IS NULL
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<ProductNodeDto>(query);
            
            return nodesData.Select(MapToNode);
        }

        public async Task<IEnumerable<BaseNode>> GetChildrenAsync(Guid parentId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM dbo.ProductNode
WHERE ParentId = @parentId
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<ProductNodeDto>(query, new { parentId });
            
            return nodesData.Select(MapToNode);
        }

        public async Task<bool> HasChildrenAsync(Guid nodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = "SELECT COUNT(1) FROM dbo.ProductNode WHERE ParentId = @nodeId";
            var count = await dbConnection.ExecuteScalarAsync<int>(query, new { nodeId });
            return count > 0;
        }

        public async Task<bool> HasItemsAsync(Guid categoryNodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Check if this is a Category node
            var node = await GetNodeByIdAsync(categoryNodeId);
            if (node?.NodeType != BaseNode.NodeTypeCategory)
            {
                return false;
            }

            var query = "SELECT COUNT(1) FROM dbo.Item WHERE CategoryId = @categoryNodeId AND Deleted = 0";
            var count = await dbConnection.ExecuteScalarAsync<int>(query, new { categoryNodeId });
            return count > 0;
        }

        public async Task<IEnumerable<BaseNode>> GetNodesByTypeAsync(string nodeType)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM dbo.ProductNode
WHERE NodeType = @nodeType
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<ProductNodeDto>(query, new { nodeType });
            
            return nodesData.Select(MapToNode);
        }

        public async Task<IEnumerable<BaseNode>> GetCategoryNodesAsync()
        {
            return await GetNodesByTypeAsync("Category");
        }

        private async Task ValidateNoCircularReferenceAsync(Guid nodeId, Guid proposedParentId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Check if proposedParentId is a descendant of nodeId
            var query = @"
WITH NodeAncestors AS (
    SELECT Id, ParentId
    FROM dbo.ProductNode
    WHERE Id = @proposedParentId
    
    UNION ALL
    
    SELECT n.Id, n.ParentId
    FROM dbo.ProductNode n
    INNER JOIN NodeAncestors na ON n.Id = na.ParentId
)
SELECT COUNT(1)
FROM NodeAncestors
WHERE Id = @nodeId";

            var count = await dbConnection.ExecuteScalarAsync<int>(query, new { nodeId, proposedParentId });
            
            if (count > 0)
            {
                throw new InvalidOperationException("Setting this parent would create a circular reference.");
            }
        }

        private static BaseNode MapToNode(ProductNodeDto dto)
        {
            BaseNode node = dto.NodeType switch
            {
                var type when type == BaseNode.NodeTypeDepartement => new DepartementNode(),
                var type when type == BaseNode.NodeTypeNavigation => new NavigationNode(),
                var type when type == BaseNode.NodeTypeCategory => new CategoryNode(),
                _ => throw new InvalidOperationException($"Unknown NodeType: {dto.NodeType}")
            };

            node.Id = dto.Id;
            node.Name_en = dto.Name_en;
            node.Name_fr = dto.Name_fr;
            node.ParentId = dto.ParentId;
            node.IsActive = dto.IsActive;
            node.SortOrder = dto.SortOrder;
            node.CreatedAt = dto.CreatedAt;
            node.UpdatedAt = dto.UpdatedAt;

            return node;
        }

        private sealed class ProductNodeDto
        {
            public Guid Id { get; set; }
            public string Name_en { get; set; } = string.Empty;
            public string Name_fr { get; set; } = string.Empty;
            public string NodeType { get; set; } = string.Empty;
            public Guid? ParentId { get; set; }
            public bool IsActive { get; set; }
            public int? SortOrder { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
}
