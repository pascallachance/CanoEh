using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;

namespace Infrastructure.Repositories.Tests
{
    /// <summary>
    /// Integration tests for ItemVariantExtraAttributeRepository that validate
    /// the actual SQL/Dapper implementation against a real database.
    /// These tests use LocalDB to execute queries end-to-end.
    /// Note: These tests are skipped on non-Windows platforms where LocalDB is not available.
    /// </summary>
    [Collection("Database")]
    public class ItemVariantExtraAttributeRepositoryIntegrationShould : IDisposable
    {
        private readonly string _connectionString;
        private readonly ItemVariantExtraAttributeRepository? _repository;
        private readonly List<Guid> _testItemIds;
        private readonly List<Guid> _testItemVariantIds;

        public ItemVariantExtraAttributeRepositoryIntegrationShould()
        {
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CanoEh;Trusted_Connection=True;";
            _testItemIds = new List<Guid>();
            _testItemVariantIds = new List<Guid>();
            
            if (IsLocalDbAvailable())
            {
                _repository = new ItemVariantExtraAttributeRepository(_connectionString);
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
        public async Task GetAttributesByVariantIdAsync_ShouldReturnAttributes_WhenAttributesExist()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var itemVariantId = await CreateTestItemVariantAsync();
            var attribute1 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId,
                Name_en = "Serial Number",
                Name_fr = "Numéro de série",
                Value_en = "SN-123456",
                Value_fr = "SN-123456"
            };
            var attribute2 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId,
                Name_en = "Batch Code",
                Name_fr = "Code de lot",
                Value_en = "BATCH-789",
                Value_fr = "LOT-789"
            };
            var attribute3 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId,
                Name_en = "Expiry Date",
                Name_fr = "Date d'expiration",
                Value_en = "2025-12-31",
                Value_fr = "2025-12-31"
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);
            await _repository!.AddAsync(attribute3);

            // Act
            var result = await _repository!.GetAttributesByVariantIdAsync(itemVariantId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            
            // Verify all attributes are returned
            Assert.Contains(resultList, a => a.Name_en == "Serial Number");
            Assert.Contains(resultList, a => a.Name_en == "Batch Code");
            Assert.Contains(resultList, a => a.Name_en == "Expiry Date");
        }

        [Fact]
        public async Task GetAttributesByVariantIdAsync_ShouldReturnEmpty_WhenNoAttributesExist()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var itemVariantId = await CreateTestItemVariantAsync();

            // Act
            var result = await _repository!.GetAttributesByVariantIdAsync(itemVariantId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAttributesByVariantIdAsync_ShouldReturnOnlyMatchingVariantAttributes()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var itemVariantId1 = await CreateTestItemVariantAsync();
            var itemVariantId2 = await CreateTestItemVariantAsync();
            
            var attribute1 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId1,
                Name_en = "Serial Number",
                Value_en = "SN-111"
            };
            var attribute2 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId2,
                Name_en = "Serial Number",
                Value_en = "SN-222"
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);

            // Act
            var result = await _repository!.GetAttributesByVariantIdAsync(itemVariantId1);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("SN-111", resultList[0].Value_en);
        }

        [Fact]
        public async Task DeleteAttributesByVariantIdAsync_ShouldReturnTrue_WhenAttributesDeleted()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var itemVariantId = await CreateTestItemVariantAsync();
            var attribute1 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId,
                Name_en = "Test Attribute 1",
                Name_fr = "Attribut de test 1",
                Value_en = "Value 1",
                Value_fr = "Valeur 1"
            };
            var attribute2 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId,
                Name_en = "Test Attribute 2",
                Name_fr = "Attribut de test 2",
                Value_en = "Value 2",
                Value_fr = "Valeur 2"
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);

            // Act
            var result = await _repository!.DeleteAttributesByVariantIdAsync(itemVariantId);

            // Assert
            Assert.True(result);

            // Verify deletion
            var remainingAttributes = await _repository!.GetAttributesByVariantIdAsync(itemVariantId);
            Assert.Empty(remainingAttributes);
        }

        [Fact]
        public async Task DeleteAttributesByVariantIdAsync_ShouldReturnFalse_WhenNoAttributesFound()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var itemVariantId = await CreateTestItemVariantAsync();

            // Act
            var result = await _repository!.DeleteAttributesByVariantIdAsync(itemVariantId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAttributesByVariantIdAsync_ShouldOnlyDeleteMatchingVariantAttributes()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var itemVariantId1 = await CreateTestItemVariantAsync();
            var itemVariantId2 = await CreateTestItemVariantAsync();
            
            var attribute1 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId1,
                Name_en = "Attribute 1",
                Value_en = "Value 1"
            };
            var attribute2 = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId2,
                Name_en = "Attribute 2",
                Value_en = "Value 2"
            };

            await _repository!.AddAsync(attribute1);
            await _repository!.AddAsync(attribute2);

            // Act
            var result = await _repository!.DeleteAttributesByVariantIdAsync(itemVariantId1);

            // Assert
            Assert.True(result);

            // Verify only variant 1's attributes were deleted
            var variant1Attributes = await _repository!.GetAttributesByVariantIdAsync(itemVariantId1);
            Assert.Empty(variant1Attributes);
            
            var variant2Attributes = await _repository!.GetAttributesByVariantIdAsync(itemVariantId2);
            Assert.Single(variant2Attributes);
            Assert.Equal("Value 2", variant2Attributes.First().Value_en);
        }

        [Fact]
        public async Task GetAttributesByVariantIdAsync_ShouldHandleNullOptionalFields()
        {
            if (!IsLocalDbAvailable())
            {
                // Skip test on non-Windows platforms
                return;
            }
            
            // Arrange
            var itemVariantId = await CreateTestItemVariantAsync();
            var attribute = new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = itemVariantId,
                Name_en = "Serial Number",
                Name_fr = null,  // Optional field
                Value_en = null,  // Optional field
                Value_fr = null   // Optional field
            };

            await _repository!.AddAsync(attribute);

            // Act
            var result = await _repository!.GetAttributesByVariantIdAsync(itemVariantId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Serial Number", resultList[0].Name_en);
            Assert.Null(resultList[0].Name_fr);
            Assert.Null(resultList[0].Value_en);
            Assert.Null(resultList[0].Value_fr);
        }

        /// <summary>
        /// Creates a test Item and ItemVariant in the database.
        /// Returns the Id of the created ItemVariant and tracks both for cleanup.
        /// </summary>
        private async Task<Guid> CreateTestItemVariantAsync()
        {
            var itemId = Guid.NewGuid();
            var itemVariantId = Guid.NewGuid();
            _testItemIds.Add(itemId);
            _testItemVariantIds.Add(itemVariantId);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Insert a test item first
            var insertItemQuery = @"
                INSERT INTO dbo.Item (Id, SellerID, Name_en, Name_fr, CategoryID, CreatedAt, Deleted)
                VALUES (@Id, @SellerID, @Name_en, @Name_fr, NULL, @CreatedAt, 0)";
            
            using var itemCommand = new SqlCommand(insertItemQuery, connection);
            itemCommand.Parameters.AddWithValue("@Id", itemId);
            // Use a fake GUID for SellerID - in real scenarios this would be a valid user
            itemCommand.Parameters.AddWithValue("@SellerID", Guid.NewGuid());
            itemCommand.Parameters.AddWithValue("@Name_en", $"Test Item {itemId}");
            itemCommand.Parameters.AddWithValue("@Name_fr", $"Article de test {itemId}");
            itemCommand.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
            
            await itemCommand.ExecuteNonQueryAsync();

            // Insert a test ItemVariant
            var insertVariantQuery = @"
                INSERT INTO dbo.ItemVariant (Id, ItemId, Price, StockQuantity, Deleted)
                VALUES (@Id, @ItemId, @Price, @StockQuantity, 0)";
            
            using var variantCommand = new SqlCommand(insertVariantQuery, connection);
            variantCommand.Parameters.AddWithValue("@Id", itemVariantId);
            variantCommand.Parameters.AddWithValue("@ItemId", itemId);
            variantCommand.Parameters.AddWithValue("@Price", 99.99m);
            variantCommand.Parameters.AddWithValue("@StockQuantity", 100);
            
            await variantCommand.ExecuteNonQueryAsync();
            
            return itemVariantId;
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
            foreach (var itemVariantId in _testItemVariantIds)
            {
                try
                {
                    using var connection = new SqlConnection(_connectionString);
                    connection.Open();
                    
                    // Delete ItemVariantExtraAttributes first (FK constraint)
                    var deleteAttributesQuery = "DELETE FROM dbo.ItemVariantExtraAttribute WHERE ItemVariantId = @ItemVariantId";
                    using var deleteAttrCmd = new SqlCommand(deleteAttributesQuery, connection);
                    deleteAttrCmd.Parameters.AddWithValue("@ItemVariantId", itemVariantId);
                    deleteAttrCmd.ExecuteNonQuery();
                    
                    // Delete the ItemVariant
                    var deleteVariantQuery = "DELETE FROM dbo.ItemVariant WHERE Id = @Id";
                    using var deleteVariantCmd = new SqlCommand(deleteVariantQuery, connection);
                    deleteVariantCmd.Parameters.AddWithValue("@Id", itemVariantId);
                    deleteVariantCmd.ExecuteNonQuery();
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }

            // Clean up test items
            foreach (var itemId in _testItemIds)
            {
                try
                {
                    using var connection = new SqlConnection(_connectionString);
                    connection.Open();
                    
                    // Delete the Item
                    var deleteItemQuery = "DELETE FROM dbo.Item WHERE Id = @Id";
                    using var deleteItemCmd = new SqlCommand(deleteItemQuery, connection);
                    deleteItemCmd.Parameters.AddWithValue("@Id", itemId);
                    deleteItemCmd.ExecuteNonQuery();
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
