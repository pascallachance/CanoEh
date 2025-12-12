using Helpers.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests
{
    public class LocalFileStorageServiceShould : IDisposable
    {
        private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;
        private readonly string _testContentRoot;
        private readonly LocalFileStorageService _service;

        public LocalFileStorageServiceShould()
        {
            _mockLogger = new Mock<ILogger<LocalFileStorageService>>();
            _testContentRoot = Path.Combine(Path.GetTempPath(), "CanoEhTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_testContentRoot);
            _service = new LocalFileStorageService(_testContentRoot, _mockLogger.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testContentRoot))
            {
                try
                {
                    Directory.Delete(_testContentRoot, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public async Task UploadFile_ReturnSuccess_WhenValidImageProvided()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var fileName = "test-image.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Contains("/uploads/", result.Value);
        }

        [Fact]
        public async Task UploadFile_ReturnFailure_WhenFileIsNull()
        {
            // Act
            var result = await _service.UploadFileAsync(null!);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("empty or not provided", result.Error);
        }

        [Fact]
        public async Task UploadFile_ReturnFailure_WhenFileIsEmpty()
        {
            // Arrange
            var content = Array.Empty<byte>();
            var fileName = "empty.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("empty or not provided", result.Error);
        }

        [Fact]
        public async Task UploadFile_ReturnFailure_WhenInvalidFileType()
        {
            // Arrange
            var content = "fake content"u8.ToArray();
            var fileName = "document.pdf";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Invalid file", result.Error);
        }

        [Fact]
        public async Task UploadFile_ReturnFailure_WhenFileSizeExceedsLimit()
        {
            // Arrange
            var content = new byte[6 * 1024 * 1024]; // 6MB - exceeds 5MB limit
            var fileName = "large-image.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("exceeds the maximum allowed size", result.Error);
        }

        [Fact]
        public async Task UploadFile_CreateUniqueFileName_WhenNotProvided()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var fileName = "test-image.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            // Check that a GUID-like name was generated
            var urlParts = result.Value.Split('/');
            var generatedFileName = urlParts[^1];
            Assert.Matches(@"^[a-f0-9-]+\.jpg$", generatedFileName);
        }

        [Fact]
        public async Task UploadFile_UseCustomFileName_WhenProvided()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var fileName = "test-image.jpg";
            var customFileName = "item_12345678-1234-1234-1234-123456789012";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile, customFileName);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Contains(customFileName, result.Value);
        }

        [Fact]
        public void GetFileUrl_ReturnCorrectUrl_ForFileName()
        {
            // Arrange
            var fileName = "test-image.jpg";

            // Act
            var url = _service.GetFileUrl(fileName);

            // Assert
            Assert.Equal("/uploads/test-image.jpg", url);
        }

        [Fact]
        public async Task DeleteFile_ReturnSuccess_WhenFileExists()
        {
            // Arrange - first upload a file
            var content = "fake image content"u8.ToArray();
            var fileName = "test-image.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            var uploadResult = await _service.UploadFileAsync(formFile, "test-delete");
            Assert.True(uploadResult.IsSuccess);

            // Extract the filename from the URL
            var url = uploadResult.Value!;
            var fileNameToDelete = url.Split('/').Last();

            // Act
            var deleteResult = await _service.DeleteFileAsync(fileNameToDelete);

            // Assert
            Assert.True(deleteResult.IsSuccess);
        }

        [Fact]
        public async Task DeleteFile_ReturnFailure_WhenFileDoesNotExist()
        {
            // Act
            var result = await _service.DeleteFileAsync("non-existent-file.jpg");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("File not found", result.Error);
        }

        [Fact]
        public async Task DeleteFile_ReturnFailure_WhenFileNameIsEmpty()
        {
            // Act
            var result = await _service.DeleteFileAsync(string.Empty);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("File path is required", result.Error);
        }

        [Fact]
        public async Task UploadFile_CreateSubdirectories_WhenSubPathProvided()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var fileName = "test-image.jpg";
            var subPath = "company123/variant456";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile, "test", subPath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Contains("company123/variant456", result.Value);
        }

        [Fact]
        public async Task UploadFile_ReturnFailure_WhenSubPathContainsPathTraversal()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var fileName = "test-image.jpg";
            var subPath = "../../../etc/passwd";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile, "test", subPath);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Invalid subpath", result.Error);
        }

        [Fact]
        public async Task UploadFile_FollowHierarchicalStructure_ForItemVariant()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var companyId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var subPath = $"{companyId}/{variantId}";
            var fileName = $"{variantId}_thumb";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile, fileName, subPath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Contains($"{companyId}/{variantId}/{variantId}_thumb", result.Value);
        }

        [Fact]
        public async Task UploadFile_FollowHierarchicalStructure_ForCompanyLogo()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var companyId = Guid.NewGuid();
            var subPath = companyId.ToString();
            var fileName = $"{companyId}_logo";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile, fileName, subPath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Contains($"{companyId}/{companyId}_logo", result.Value);
        }

        [Fact]
        public async Task DeleteFile_DeleteFromSubdirectory_WhenFileInSubPath()
        {
            // Arrange - first upload a file to a subdirectory
            var content = "fake image content"u8.ToArray();
            var subPath = "company123/variant456";
            var fileName = "test-image";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            var uploadResult = await _service.UploadFileAsync(formFile, fileName, subPath);
            Assert.True(uploadResult.IsSuccess);

            // Extract the relative path from the URL (remove /uploads/ prefix)
            var url = uploadResult.Value!;
            var relativePath = url.Replace("/uploads/", "");

            // Act
            var deleteResult = await _service.DeleteFileAsync(relativePath);

            // Assert
            Assert.True(deleteResult.IsSuccess);
        }

        [Fact]
        public async Task UploadFile_VerifyFileExistsOnDisk_AfterSuccessfulUpload()
        {
            // Arrange
            var content = "fake image content"u8.ToArray();
            var companyId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var subPath = $"{companyId}/{variantId}";
            var fileName = $"{variantId}_thumb";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            // Act
            var result = await _service.UploadFileAsync(formFile, fileName, subPath);

            // Assert
            Assert.True(result.IsSuccess);
            
            // Verify the file actually exists on disk
            var expectedPath = Path.Combine(_testContentRoot, "wwwroot", "uploads", subPath, $"{fileName}.jpg");
            Assert.True(File.Exists(expectedPath), $"Expected file to exist at {expectedPath}");
            
            // Verify file content
            var fileContent = await File.ReadAllBytesAsync(expectedPath);
            Assert.Equal(content, fileContent);
        }
    }
}
