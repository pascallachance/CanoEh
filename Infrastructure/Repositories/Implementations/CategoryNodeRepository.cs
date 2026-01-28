using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories.Implementations
{
    public class CategoryNodeRepository(string connectionString) : GenericRepository<BaseNode>(connectionString), ICategoryNodeRepository
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
INSERT INTO dbo.CategoryNode (Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt)
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
            if (entity.NodeType == BaseNode.NodeTypeCategory)
            {
                var hasItems = await HasItemsAsync(entity.Id);
                if (hasItems)
                {
                    throw new InvalidOperationException("Cannot delete category node that has items.");
                }
            }

            var query = "DELETE FROM dbo.CategoryNode WHERE Id = @Id";
            await dbConnection.ExecuteAsync(query, new { entity.Id });
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.CategoryNode WHERE Id = @id", new { id });
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
FROM dbo.CategoryNode
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<CategoryNodeDto>(query);
            
            return nodesData.Select(MapToNode);
        }

        public override async Task<BaseNode> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var node = await GetNodeByIdAsync(id);
            return node ?? throw new InvalidOperationException($"CategoryNode with id {id} not found");
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
UPDATE dbo.CategoryNode
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

        public async Task<BaseNode?> GetNodeByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = @"
SELECT Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt, UpdatedAt
FROM dbo.CategoryNode
WHERE Id = @id";

            var dto = await dbConnection.QueryFirstOrDefaultAsync<CategoryNodeDto>(query, new { id });
            
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
FROM dbo.CategoryNode
WHERE ParentId IS NULL
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<CategoryNodeDto>(query);
            
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
FROM dbo.CategoryNode
WHERE ParentId = @parentId
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<CategoryNodeDto>(query, new { parentId });
            
            return nodesData.Select(MapToNode);
        }

        public async Task<bool> HasChildrenAsync(Guid nodeId)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            var query = "SELECT COUNT(1) FROM dbo.CategoryNode WHERE ParentId = @nodeId";
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

            var query = "SELECT COUNT(1) FROM dbo.Item WHERE CategoryID = @categoryNodeId AND Deleted = 0";
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
FROM dbo.CategoryNode
WHERE NodeType = @nodeType
ORDER BY SortOrder, Name_en";

            var nodesData = await dbConnection.QueryAsync<CategoryNodeDto>(query, new { nodeType });
            
            return nodesData.Select(MapToNode);
        }

        public async Task<IEnumerable<BaseNode>> GetCategoryNodesAsync()
        {
            return await GetNodesByTypeAsync(BaseNode.NodeTypeCategory);
        }

        public async Task<(BaseNode node, IEnumerable<CategoryMandatoryAttribute> attributes)> AddNodeWithAttributesAsync(
            BaseNode node, 
            IEnumerable<CategoryMandatoryAttribute> attributes)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            // Validate node type
            if (node.NodeType != BaseNode.NodeTypeDepartement && node.NodeType != BaseNode.NodeTypeNavigation && node.NodeType != BaseNode.NodeTypeCategory)
            {
                throw new ArgumentException($"Invalid NodeType. Must be '{BaseNode.NodeTypeDepartement}', '{BaseNode.NodeTypeNavigation}', or '{BaseNode.NodeTypeCategory}'.");
            }

            // Validate ParentId constraints
            if (node.NodeType == BaseNode.NodeTypeDepartement && node.ParentId.HasValue)
            {
                throw new InvalidOperationException("Departement nodes cannot have a parent.");
            }

            if (node.NodeType != BaseNode.NodeTypeDepartement && !node.ParentId.HasValue)
            {
                throw new InvalidOperationException($"{node.NodeType} nodes must have a parent.");
            }

            using var transaction = dbConnection.BeginTransaction();
            try
            {
                // Insert the CategoryNode
                var nodeQuery = @"
INSERT INTO dbo.CategoryNode (Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt)
VALUES (@Id, @Name_en, @Name_fr, @NodeType, @ParentId, @IsActive, @SortOrder, @CreatedAt)";

                var nodeParameters = new
                {
                    node.Id,
                    node.Name_en,
                    node.Name_fr,
                    node.NodeType,
                    node.ParentId,
                    node.IsActive,
                    node.SortOrder,
                    node.CreatedAt
                };

                await dbConnection.ExecuteAsync(nodeQuery, nodeParameters, transaction);

                // Insert the CategoryMandatoryAttributes if any
                var createdAttributes = new List<CategoryMandatoryAttribute>();
                if (attributes != null && attributes.Any())
                {
                    var attributeQuery = @"
INSERT INTO dbo.CategoryMandatoryAttribute (Id, CategoryNodeId, Name_en, Name_fr, AttributeType, SortOrder)
VALUES (@Id, @CategoryNodeId, @Name_en, @Name_fr, @AttributeType, @SortOrder)";

                    foreach (var attribute in attributes)
                    {
                        var attributeParameters = new
                        {
                            attribute.Id,
                            attribute.CategoryNodeId,
                            attribute.Name_en,
                            attribute.Name_fr,
                            attribute.AttributeType,
                            attribute.SortOrder
                        };

                        await dbConnection.ExecuteAsync(attributeQuery, attributeParameters, transaction);
                        createdAttributes.Add(attribute);
                    }
                }

                transaction.Commit();
                return (node, createdAttributes);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<BaseNode>> AddMultipleNodesWithAttributesAsync(
            IEnumerable<(BaseNode node, IEnumerable<CategoryMandatoryAttribute> attributes)> nodesWithAttributes)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }

            using var transaction = dbConnection.BeginTransaction();
            try
            {
                var createdNodes = new List<BaseNode>();
                var allNodeParameters = new List<object>();
                var allAttributeParameters = new List<object>();

                foreach (var (node, attributes) in nodesWithAttributes)
                {
                    // Validate node type
                    if (node.NodeType != BaseNode.NodeTypeDepartement && node.NodeType != BaseNode.NodeTypeNavigation && node.NodeType != BaseNode.NodeTypeCategory)
                    {
                        throw new ArgumentException($"Invalid NodeType. Must be '{BaseNode.NodeTypeDepartement}', '{BaseNode.NodeTypeNavigation}', or '{BaseNode.NodeTypeCategory}'.");
                    }

                    // Validate ParentId constraints
                    if (node.NodeType == BaseNode.NodeTypeDepartement && node.ParentId.HasValue)
                    {
                        throw new InvalidOperationException("Departement nodes cannot have a parent.");
                    }

                    if (node.NodeType != BaseNode.NodeTypeDepartement && !node.ParentId.HasValue)
                    {
                        throw new InvalidOperationException($"{node.NodeType} nodes must have a parent.");
                    }

                    // Collect node parameters for batch insert
                    allNodeParameters.Add(new
                    {
                        node.Id,
                        node.Name_en,
                        node.Name_fr,
                        node.NodeType,
                        node.ParentId,
                        node.IsActive,
                        node.SortOrder,
                        node.CreatedAt
                    });

                    // Collect attribute parameters for batch insert
                        var attributeParameters = attributes.Select(attribute => new
                        {
                            attribute.Id,
                            attribute.CategoryNodeId,
                            attribute.Name_en,
                            attribute.Name_fr,
                            attribute.AttributeType,
                            attribute.SortOrder
                        });

                        await dbConnection.ExecuteAsync(attributeQuery, attributeParameters, transaction);
                    }

                    createdNodes.Add(node);
                }

                // Batch insert all nodes in a single database roundtrip
                if (allNodeParameters.Any())
                {
                    var nodeQuery = @"
INSERT INTO dbo.CategoryNode (Id, Name_en, Name_fr, NodeType, ParentId, IsActive, SortOrder, CreatedAt)
VALUES (@Id, @Name_en, @Name_fr, @NodeType, @ParentId, @IsActive, @SortOrder, @CreatedAt)";
                    
                    await dbConnection.ExecuteAsync(nodeQuery, allNodeParameters, transaction);
                }

                // Batch insert all attributes in a single database roundtrip
                if (allAttributeParameters.Any())
                {
                    var attributeQuery = @"
INSERT INTO dbo.CategoryMandatoryAttribute (Id, CategoryNodeId, Name_en, Name_fr, AttributeType, SortOrder)
VALUES (@Id, @CategoryNodeId, @Name_en, @Name_fr, @AttributeType, @SortOrder)";
                    
                    await dbConnection.ExecuteAsync(attributeQuery, allAttributeParameters, transaction);
                }

                transaction.Commit();
                return createdNodes;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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
    FROM dbo.CategoryNode
    WHERE Id = @proposedParentId
    
    UNION ALL
    
    SELECT n.Id, n.ParentId
    FROM dbo.CategoryNode n
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

        private static BaseNode MapToNode(CategoryNodeDto dto)
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

        private sealed class CategoryNodeDto
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
