using System.Security.Claims;
using API.Controllers;
using Domain.Models.Responses;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests
{
    /// <summary>
    /// Integration tests for the image upload workflow including ItemController and LocalFileStorageService
    /// </summary>
    public class ImageUploadIntegrationShould : IDisposable
    {
        private readonly Mock<IItemService> _mockItemService;
        private readonly Mock<ILogger<ItemController>> _mockControllerLogger;
        private readonly Mock<ILogger<LocalFileStorageService>> _mockStorageLogger;
        private readonly string _testContentRoot;
        private readonly LocalFileStorageService _fileStorageService;
        private readonly ItemController _controller;

        public ImageUploadIntegrationShould()
        {
            _mockItemService = new Mock<IItemService>();
            _mockControllerLogger = new Mock<ILogger<ItemController>>();
            _mockStorageLogger = new Mock<ILogger<LocalFileStorageService>>();
            
            // Create a test directory for file uploads
            _testContentRoot = Path.Combine(Path.GetTempPath(), "CanoEhUploadTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_testContentRoot);
            
            // Create real file storage service
            _fileStorageService = new LocalFileStorageService(_testContentRoot, _mockStorageLogger.Object);
            
            // Create controller with real file storage service
            _controller = new ItemController(_mockItemService.Object, _fileStorageService, _mockControllerLogger.Object);
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
        public async Task UploadImage_CreateDirectoryStructure_WhenUploadingThumbnail()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var userId = companyId; // Using same ID for simplicity
            
            // Mock the item service to return an item
            var mockItem = new GetItemResponse
            {
                Id = Guid.NewGuid(),
                SellerID = companyId,
                Name_en = "Test Item",
                Name_fr = "Article de test",
                CategoryID = Guid.NewGuid()
            };
            
            _mockItemService
                .Setup(s => s.GetItemByVariantIdAsync(variantId, userId))
                .ReturnsAsync(Result.Success(mockItem));
            
            // Create a fake image file
            var content = "fake image content"u8.ToArray();
            var fileName = "test-thumbnail.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            
            // Set up authentication context
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.UploadImage(formFile, variantId, "thumbnail", 1);

            // Assert - Check the HTTP response
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.NotNull(okResult.Value);
            
            // Verify the file was created on disk in the correct directory structure
            var expectedDirectory = Path.Combine(_testContentRoot, "wwwroot", "uploads", companyId.ToString(), variantId.ToString());
            Assert.True(Directory.Exists(expectedDirectory), $"Expected directory to exist: {expectedDirectory}");
            
            var expectedFilePath = Path.Combine(expectedDirectory, $"{variantId}_thumb.jpg");
            Assert.True(File.Exists(expectedFilePath), $"Expected file to exist: {expectedFilePath}");
            
            // Verify file content
            var savedContent = await File.ReadAllBytesAsync(expectedFilePath);
            Assert.Equal(content, savedContent);
        }

        [Fact]
        public async Task UploadImage_CreateDirectoryStructure_WhenUploadingMultipleImages()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var userId = companyId;
            
            var mockItem = new GetItemResponse
            {
                Id = Guid.NewGuid(),
                SellerID = companyId,
                Name_en = "Test Item",
                Name_fr = "Article de test",
                CategoryID = Guid.NewGuid()
            };
            
            _mockItemService
                .Setup(s => s.GetItemByVariantIdAsync(variantId, userId))
                .ReturnsAsync(Result.Success(mockItem));
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act - Upload 3 images
            for (int i = 1; i <= 3; i++)
            {
                var contentText = $"fake image content {i}";
                var content = System.Text.Encoding.UTF8.GetBytes(contentText);
                var fileName = $"test-image-{i}.jpg";
                using var stream = new MemoryStream(content);
                var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };
                
                var result = await _controller.UploadImage(formFile, variantId, "image", i);
                
                // Assert each upload
                Assert.IsType<OkObjectResult>(result);
            }

            // Assert - Check all files were created
            var expectedDirectory = Path.Combine(_testContentRoot, "wwwroot", "uploads", companyId.ToString(), variantId.ToString());
            Assert.True(Directory.Exists(expectedDirectory));
            
            for (int i = 1; i <= 3; i++)
            {
                var expectedFilePath = Path.Combine(expectedDirectory, $"{variantId}_{i}.jpg");
                Assert.True(File.Exists(expectedFilePath), $"Expected file to exist: {expectedFilePath}");
            }
        }

        [Fact]
        public async Task UploadImage_LogDetailedInformation_DuringUpload()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var userId = companyId;
            
            var mockItem = new GetItemResponse
            {
                Id = Guid.NewGuid(),
                SellerID = companyId,
                Name_en = "Test Item",
                Name_fr = "Article de test",
                CategoryID = Guid.NewGuid()
            };
            
            _mockItemService
                .Setup(s => s.GetItemByVariantIdAsync(variantId, userId))
                .ReturnsAsync(Result.Success(mockItem));
            
            var content = "fake image content"u8.ToArray();
            var fileName = "test-log.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            await _controller.UploadImage(formFile, variantId, "thumbnail", 1);

            // Assert - Verify logging occurred
            _mockControllerLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("UploadImage API START")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            _mockStorageLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("UploadFileAsync START")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            
            _mockStorageLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ContentRootPath")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadImage_ReturnNotFound_WhenVariantDoesNotExist()
        {
            // Arrange
            var variantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            _mockItemService
                .Setup(s => s.GetItemByVariantIdAsync(variantId, userId))
                .ReturnsAsync(Result.Failure<GetItemResponse>("Variant not found", StatusCodes.Status404NotFound));
            
            var content = "fake image content"u8.ToArray();
            var fileName = "test.jpg";
            using var stream = new MemoryStream(content);
            var formFile = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await _controller.UploadImage(formFile, variantId, "thumbnail", 1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
