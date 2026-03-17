using Infrastructure.Data;
using Infrastructure.Repositories.Implementations;
using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;

namespace Infrastructure.Repositories.Tests
{
    /// <summary>
    /// Integration tests for <see cref="ItemRepository"/> that validate the actual
    /// SQL/Dapper implementation of <see cref="ItemRepository.GetItemByIdAsync"/> against a real database.
    /// These tests require SQL Server LocalDB (Windows only) and are automatically skipped on other platforms.
    /// </summary>
    [Collection("Database")]
    public class ItemRepositoryIntegrationShould : IDisposable
    {
        private readonly string _connectionString;
        private readonly ItemRepository? _repository;
        private readonly List<Guid> _testItemIds = new();
        private readonly List<Guid> _testVariantIds = new();
        private readonly List<Guid> _testCategoryNodeIds = new();

        public ItemRepositoryIntegrationShould()
        {
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CanoEh;Trusted_Connection=True;";

            if (IsLocalDbAvailable())
            {
                _repository = new ItemRepository(_connectionString);
            }
        }

        private static bool IsLocalDbAvailable() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // ------------------------------------------------------------------ helpers

        private async Task<Guid> CreateTestCategoryNodeAsync()
        {
            var id = Guid.NewGuid();
            _testCategoryNodeIds.Add(id);
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(
                "INSERT INTO dbo.CategoryNode (Id, Name_en, Name_fr, NodeType, ParentId, SortOrder) VALUES (@Id, @Name_en, @Name_fr, 'Category', NULL, 1)",
                conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name_en", $"Test Category {id}");
            cmd.Parameters.AddWithValue("@Name_fr", $"Catégorie test {id}");
            await cmd.ExecuteNonQueryAsync();
            return id;
        }

        private async Task<Guid> CreateTestItemAsync(Guid categoryNodeId)
        {
            var id = Guid.NewGuid();
            _testItemIds.Add(id);
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(
                @"INSERT INTO dbo.Item (Id, SellerID, Name_en, Name_fr, CategoryNodeID, CreatedAt, Deleted)
                  VALUES (@Id, @SellerID, @Name_en, @Name_fr, @CategoryNodeID, GETUTCDATE(), 0)",
                conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@SellerID", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@Name_en", $"Integration Test Item {id}");
            cmd.Parameters.AddWithValue("@Name_fr", $"Article test intégration {id}");
            cmd.Parameters.AddWithValue("@CategoryNodeID", categoryNodeId);
            await cmd.ExecuteNonQueryAsync();
            return id;
        }

        private async Task<Guid> CreateTestVariantAsync(Guid itemId, string imageUrls = "/uploads/test.jpg", string sku = "INT-TEST-001")
        {
            var id = Guid.NewGuid();
            _testVariantIds.Add(id);
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(
                @"INSERT INTO dbo.ItemVariant (Id, ItemId, Price, StockQuantity, Sku, ImageUrls, Deleted)
                  VALUES (@Id, @ItemId, @Price, @StockQuantity, @Sku, @ImageUrls, 0)",
                conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@ItemId", itemId);
            cmd.Parameters.AddWithValue("@Price", 29.99m);
            cmd.Parameters.AddWithValue("@StockQuantity", 10);
            cmd.Parameters.AddWithValue("@Sku", sku);
            cmd.Parameters.AddWithValue("@ImageUrls", imageUrls);
            await cmd.ExecuteNonQueryAsync();
            return id;
        }

        private async Task CreateTestVariantAttributeAsync(Guid variantId, string nameEn, string valueEn, string? nameFr = null, string? valueFr = null)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(
                @"INSERT INTO dbo.ItemVariantAttribute (Id, ItemVariantID, AttributeName_en, AttributeName_fr, Attributes_en, Attributes_fr)
                  VALUES (@Id, @ItemVariantID, @NameEn, @NameFr, @ValueEn, @ValueFr)",
                conn);
            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@ItemVariantID", variantId);
            cmd.Parameters.AddWithValue("@NameEn", nameEn);
            cmd.Parameters.AddWithValue("@NameFr", (object?)nameFr ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ValueEn", valueEn);
            cmd.Parameters.AddWithValue("@ValueFr", (object?)valueFr ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        // ------------------------------------------------------------------ tests

        [Fact]
        public async Task GetItemByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
        {
            if (!IsLocalDbAvailable()) return;

            var result = await _repository!.GetItemByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetItemByIdAsync_ShouldReturnItemWithEmptyVariants_WhenItemHasNoVariants()
        {
            if (!IsLocalDbAvailable()) return;

            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var itemId = await CreateTestItemAsync(categoryNodeId);

            var result = await _repository!.GetItemByIdAsync(itemId);

            Assert.NotNull(result);
            Assert.Equal(itemId, result.Id);
            Assert.Empty(result.Variants);
            Assert.Empty(result.ItemVariantFeatures);
        }

        [Fact]
        public async Task GetItemByIdAsync_ShouldReturnItemWithVariants_WhenItemHasVariants()
        {
            if (!IsLocalDbAvailable()) return;

            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var itemId = await CreateTestItemAsync(categoryNodeId);
            var variantId = await CreateTestVariantAsync(itemId, "/uploads/img1.jpg,/uploads/img2.jpg");

            var result = await _repository!.GetItemByIdAsync(itemId);

            Assert.NotNull(result);
            Assert.Single(result.Variants);
            Assert.Equal(variantId, result.Variants[0].Id);
            Assert.Equal("/uploads/img1.jpg,/uploads/img2.jpg", result.Variants[0].ImageUrls);
        }

        [Fact]
        public async Task GetItemByIdAsync_ShouldReturnVariantWithAttributes_WhenVariantHasAttributes()
        {
            if (!IsLocalDbAvailable()) return;

            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var itemId = await CreateTestItemAsync(categoryNodeId);
            var variantId = await CreateTestVariantAsync(itemId);
            await CreateTestVariantAttributeAsync(variantId, "Color", "Red", "Couleur", "Rouge");

            var result = await _repository!.GetItemByIdAsync(itemId);

            Assert.NotNull(result);
            Assert.Single(result.Variants);
            Assert.Single(result.Variants[0].ItemVariantAttributes);
            Assert.Equal("Color", result.Variants[0].ItemVariantAttributes[0].AttributeName_en);
            Assert.Equal("Red", result.Variants[0].ItemVariantAttributes[0].Attributes_en);
            Assert.Equal("Couleur", result.Variants[0].ItemVariantAttributes[0].AttributeName_fr);
            Assert.Equal("Rouge", result.Variants[0].ItemVariantAttributes[0].Attributes_fr);
        }

        [Fact]
        public async Task GetItemByIdAsync_ShouldReturnMultipleVariants_WhenItemHasMultipleVariants()
        {
            if (!IsLocalDbAvailable()) return;

            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var itemId = await CreateTestItemAsync(categoryNodeId);
            var variantId1 = await CreateTestVariantAsync(itemId, "/uploads/black.jpg", "SKU-BLK");
            var variantId2 = await CreateTestVariantAsync(itemId, "/uploads/white.jpg", "SKU-WHT");
            await CreateTestVariantAttributeAsync(variantId1, "Color", "Black");
            await CreateTestVariantAttributeAsync(variantId2, "Color", "White");

            var result = await _repository!.GetItemByIdAsync(itemId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Variants.Count);
            var variantIds = result.Variants.Select(v => v.Id).ToList();
            Assert.Contains(variantId1, variantIds);
            Assert.Contains(variantId2, variantIds);
            Assert.All(result.Variants, v => Assert.Single(v.ItemVariantAttributes));
        }

        [Fact]
        public async Task GetItemByIdAsync_ShouldNotReturnDeletedVariants()
        {
            if (!IsLocalDbAvailable()) return;

            var categoryNodeId = await CreateTestCategoryNodeAsync();
            var itemId = await CreateTestItemAsync(categoryNodeId);
            var activeVariantId = await CreateTestVariantAsync(itemId, "/uploads/active.jpg", "SKU-ACTIVE");

            // Insert a deleted variant directly
            var deletedVariantId = Guid.NewGuid();
            _testVariantIds.Add(deletedVariantId);
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using var cmd = new SqlCommand(
                    "INSERT INTO dbo.ItemVariant (Id, ItemId, Price, StockQuantity, Sku, Deleted) VALUES (@Id, @ItemId, 9.99, 5, 'SKU-DEL', 1)",
                    conn);
                cmd.Parameters.AddWithValue("@Id", deletedVariantId);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                await cmd.ExecuteNonQueryAsync();
            }

            var result = await _repository!.GetItemByIdAsync(itemId);

            Assert.NotNull(result);
            Assert.Single(result.Variants);
            Assert.Equal(activeVariantId, result.Variants[0].Id);
        }

        // ------------------------------------------------------------------ cleanup

        public void Dispose()
        {
            if (!IsLocalDbAvailable())
            {
                GC.SuppressFinalize(this);
                return;
            }

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                if (_testVariantIds.Count > 0)
                {
                    // Delete variant attributes (FK)
                    foreach (var variantId in _testVariantIds)
                    {
                        using var deleteAttrCmd = new SqlCommand(
                            "DELETE FROM dbo.ItemVariantAttribute WHERE ItemVariantID = @VariantId", conn);
                        deleteAttrCmd.Parameters.AddWithValue("@VariantId", variantId);
                        deleteAttrCmd.ExecuteNonQuery();

                        using var deleteFeatCmd = new SqlCommand(
                            "DELETE FROM dbo.ItemVariantFeatures WHERE ItemVariantID = @VariantId", conn);
                        deleteFeatCmd.Parameters.AddWithValue("@VariantId", variantId);
                        deleteFeatCmd.ExecuteNonQuery();
                    }

                    // Delete variants
                    foreach (var variantId in _testVariantIds)
                    {
                        using var deleteVariantCmd = new SqlCommand(
                            "DELETE FROM dbo.ItemVariant WHERE Id = @Id", conn);
                        deleteVariantCmd.Parameters.AddWithValue("@Id", variantId);
                        deleteVariantCmd.ExecuteNonQuery();
                    }
                }

                // Delete items
                foreach (var itemId in _testItemIds)
                {
                    using var deleteItemCmd = new SqlCommand("DELETE FROM dbo.Item WHERE Id = @Id", conn);
                    deleteItemCmd.Parameters.AddWithValue("@Id", itemId);
                    deleteItemCmd.ExecuteNonQuery();
                }

                // Delete category nodes
                foreach (var categoryNodeId in _testCategoryNodeIds)
                {
                    using var deleteNodeCmd = new SqlCommand("DELETE FROM dbo.CategoryNode WHERE Id = @Id", conn);
                    deleteNodeCmd.Parameters.AddWithValue("@Id", categoryNodeId);
                    deleteNodeCmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }

            _repository?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
