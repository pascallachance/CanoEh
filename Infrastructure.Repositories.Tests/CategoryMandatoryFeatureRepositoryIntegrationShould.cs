using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;

namespace Infrastructure.Repositories.Tests
{
    /// <summary>
    /// Collection definition for database integration tests.
    /// This prevents parallel execution of database tests to avoid conflicts.
    /// </summary>
    [CollectionDefinition("Database")]
    public class DatabaseCollection
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and the collection's fixtures.
    }

    /// <summary>
    /// Integration tests for CategoryMandatoryFeatureRepository that validate
    /// the actual SQL/Dapper implementation against a real database.
    /// These tests use LocalDB to execute queries end-to-end.
    /// Note: These tests are skipped on non-Windows platforms where LocalDB is not available.
    /// </summary>
    [Collection("Database")]
    public class CategoryMandatoryFeatureRepositoryIntegrationShould : IDisposable
    {
        private readonly string _connectionString;
        private readonly CategoryMandatoryFeatureRepository? _repository;
        private readonly List<Guid> _testCategoryNodeIds;

        public CategoryMandatoryFeatureRepositoryIntegrationShould()
        {
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CanoEh;Trusted_Connection=True;";
            _testCategoryNodeIds = new List<Guid>();
            
            if (IsLocalDbAvailable())
            {
                _repository = new CategoryMandatoryFeatureRepository(_connectionString);
            }
        }

        /// <summary>
        /// Checks if LocalDB is available (Windows platform).
        /// </summary>
        private static bool IsLocalDbAvailable()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        [Fact]
        public async Task GetFeaturesByCategoryNodeIdAsync_ShouldReturnAttributesOrderedBySortOrder_WhenAttributesExist()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var attribute1 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Brand",
                Name_fr = "Marque",
                AttributeType = "string",
                SortOrder = 2
            };
            var attribute2 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Size",
                Name_fr = "Taille",
                AttributeType = "enum",
                SortOrder = 1
            };
            var attribute3 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Weight",
                Name_fr = "Poids",
                AttributeType = "int",
                SortOrder = 3
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);
            await _repository!.AddAsync(attribute3);

            // Act
            var result = await _repository!.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            
            // Verify correct ordering by SortOrder
            Assert.Equal("Size", resultList[0].Name_en);
            Assert.Equal(1, resultList[0].SortOrder);
            Assert.Equal("Brand", resultList[1].Name_en);
            Assert.Equal(2, resultList[1].SortOrder);
            Assert.Equal("Weight", resultList[2].Name_en);
            Assert.Equal(3, resultList[2].SortOrder);
        }

        [Fact]
        public async Task GetFeaturesByCategoryNodeIdAsync_ShouldReturnEmpty_WhenNoAttributesExist()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var categoryNodeId = await CreateTestCategoryNodeAsync();

            // Act
            var result = await _repository!.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteFeaturesByCategoryNodeIdAsync_ShouldReturnTrue_WhenAttributesDeleted()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var attribute1 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Test Attribute 1",
                Name_fr = "Attribut de test 1",
                AttributeType = "string",
                SortOrder = 1
            };
            var attribute2 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Test Attribute 2",
                Name_fr = "Attribut de test 2",
                AttributeType = "int",
                SortOrder = 2
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);

            // Act
            var result = await _repository!.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.True(result);

            // Verify deletion
            var remainingAttributes = await _repository!.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);
            Assert.Empty(remainingAttributes);
        }

        [Fact]
        public async Task DeleteFeaturesByCategoryNodeIdAsync_ShouldReturnFalse_WhenNoAttributesFound()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var categoryNodeId = await CreateTestCategoryNodeAsync();

            // Act
            var result = await _repository!.DeleteFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetFeaturesByCategoryNodeIdAsync_ShouldHandleNullSortOrder_ByPlacingAtEnd()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var attribute1 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "First",
                Name_fr = "Premier",
                AttributeType = "string",
                SortOrder = 1
            };
            var attribute2 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Unsorted",
                Name_fr = "Non trié",
                AttributeType = "string",
                SortOrder = null
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);

            // Act
            var result = await _repository!.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            
            // Verify attribute with SortOrder comes first
            Assert.Equal("First", resultList[0].Name_en);
            Assert.Equal(1, resultList[0].SortOrder);
            
            // Verify attribute without SortOrder comes last
            Assert.Equal("Unsorted", resultList[1].Name_en);
            Assert.Null(resultList[1].SortOrder);
        }

        [Fact]
        public async Task GetFeaturesByCategoryNodeIdAsync_ShouldSortByNameWhenSortOrderIsSame()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var attribute1 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Zebra",
                Name_fr = "Zèbre",
                AttributeType = "string",
                SortOrder = 1
            };
            var attribute2 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Apple",
                Name_fr = "Pomme",
                AttributeType = "string",
                SortOrder = 1
            };
            var attribute3 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Banana",
                Name_fr = "Banane",
                AttributeType = "string",
                SortOrder = 1
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);
            await _repository!.AddAsync(attribute3);

            // Act
            var result = await _repository!.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            
            // Verify alphabetical ordering by Name_en when SortOrder is the same
            Assert.Equal("Apple", resultList[0].Name_en);
            Assert.Equal("Banana", resultList[1].Name_en);
            Assert.Equal("Zebra", resultList[2].Name_en);
            
            // All should have SortOrder = 1
            Assert.All(resultList, attr => Assert.Equal(1, attr.SortOrder));
        }

        [Fact]
        public async Task GetFeaturesByCategoryNodeIdAsync_ShouldSortByNameWhenMultipleNullSortOrders()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var attribute1 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Zebra",
                Name_fr = "Zèbre",
                AttributeType = "string",
                SortOrder = null
            };
            var attribute2 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "Apple",
                Name_fr = "Pomme",
                AttributeType = "string",
                SortOrder = null
            };
            var attribute3 = new CategoryMandatoryFeature
            {
                Id = Guid.NewGuid(),
                CategoryNodeId = categoryNodeId,
                Name_en = "First",
                Name_fr = "Premier",
                AttributeType = "string",
                SortOrder = 1
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);
            await _repository!.AddAsync(attribute3);

            // Act
            var result = await _repository!.GetFeaturesByCategoryNodeIdAsync(categoryNodeId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            
            // Verify attribute with SortOrder comes first
            Assert.Equal("First", resultList[0].Name_en);
            Assert.Equal(1, resultList[0].SortOrder);
            
            // Verify NULL SortOrder attributes are sorted alphabetically by Name_en
            Assert.Equal("Apple", resultList[1].Name_en);
            Assert.Null(resultList[1].SortOrder);
            Assert.Equal("Zebra", resultList[2].Name_en);
            Assert.Null(resultList[2].SortOrder);
        }

        /// <summary>
        /// Creates a test CategoryNode (database table CategoryNode with NodeType='Category') in the database.
        /// Returns the Id of the created node and tracks it for cleanup.
        /// </summary>
        private async Task<Guid> CreateTestCategoryNodeAsync()
        {
            var categoryNodeId = Guid.NewGuid();
            _testCategoryNodeIds.Add(categoryNodeId);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Insert a test node into CategoryNode table with NodeType='Category'
            var insertQuery = @"
                INSERT INTO dbo.CategoryNode (Id, Name_en, Name_fr, NodeType, ParentId, SortOrder)
                VALUES (@Id, @Name_en, @Name_fr, @NodeType, NULL, 1)";
            
            using var command = new SqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@Id", categoryNodeId);
            command.Parameters.AddWithValue("@Name_en", $"Test Category {categoryNodeId}");
            command.Parameters.AddWithValue("@Name_fr", $"Catégorie de test {categoryNodeId}");
            command.Parameters.AddWithValue("@NodeType", "Category");
            
            await command.ExecuteNonQueryAsync();
            
            return categoryNodeId;
        }

        /// <summary>
        /// Cleanup test data after each test
        /// </summary>
        public void Dispose()
        {
            // Skip cleanup if LocalDB is not available
            if (!IsLocalDbAvailable())
            {
                GC.SuppressFinalize(this);
                return;
            }

            // Clean up test data
            foreach (var categoryNodeId in _testCategoryNodeIds)
            {
                try
                {
                    using var connection = new SqlConnection(_connectionString);
                    connection.Open();
                    
                    // Delete CategoryMandatoryFeatures first (FK constraint)
                    var deleteAttributesQuery = "DELETE FROM dbo.CategoryMandatoryFeature WHERE CategoryNodeId = @CategoryNodeId";
                    using var deleteAttrCmd = new SqlCommand(deleteAttributesQuery, connection);
                    deleteAttrCmd.Parameters.AddWithValue("@CategoryNodeId", categoryNodeId);
                    deleteAttrCmd.ExecuteNonQuery();
                    
                    // Delete the node from CategoryNode table
                    var deleteNodeQuery = "DELETE FROM dbo.CategoryNode WHERE Id = @Id";
                    using var deleteNodeCmd = new SqlCommand(deleteNodeQuery, connection);
                    deleteNodeCmd.Parameters.AddWithValue("@Id", categoryNodeId);
                    deleteNodeCmd.ExecuteNonQuery();
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }

            // Dispose the repository to release database connections
            _repository?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
