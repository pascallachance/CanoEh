using Helpers.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _contentRootPath;
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _uploadFolder = "uploads";

        public LocalFileStorageService(string contentRootPath, ILogger<LocalFileStorageService> logger)
        {
            _contentRootPath = contentRootPath;
            _logger = logger;
        }

        public async Task<Result<string>> UploadFileAsync(IFormFile file, string? fileName = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Result.Failure<string>("File is empty or not provided.", StatusCodes.Status400BadRequest);
                }

                // Validate file type (images only)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Result.Failure<string>($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}", StatusCodes.Status400BadRequest);
                }

                // Validate file size (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                {
                    return Result.Failure<string>("File size exceeds the maximum allowed size of 5MB.", StatusCodes.Status400BadRequest);
                }

                // Generate unique file name if not provided
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = $"{Guid.NewGuid()}{fileExtension}";
                }
                else if (!fileName.EndsWith(fileExtension))
                {
                    fileName = $"{fileName}{fileExtension}";
                }

                // Ensure the upload directory exists
                var uploadsPath = Path.Combine(_contentRootPath, "wwwroot", _uploadFolder);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                    _logger.LogInformation("Created uploads directory at {Path}", uploadsPath);
                }

                // Save the file
                var filePath = Path.Combine(uploadsPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded successfully: {FileName}", fileName);

                // Return the URL
                var fileUrl = GetFileUrl(fileName);
                return Result.Success(fileUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return Result.Failure<string>($"Error uploading file: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public string GetFileUrl(string fileName)
        {
            // Return a relative URL that can be used by the frontend
            return $"/{_uploadFolder}/{fileName}";
        }

        public Task<Result> DeleteFileAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Task.FromResult(Result.Failure("File name is required.", StatusCodes.Status400BadRequest));
                }

                var filePath = Path.Combine(_contentRootPath, "wwwroot", _uploadFolder, fileName);
                
                if (!File.Exists(filePath))
                {
                    return Task.FromResult(Result.Failure("File not found.", StatusCodes.Status404NotFound));
                }

                File.Delete(filePath);
                _logger.LogInformation("File deleted successfully: {FileName}", fileName);

                return Task.FromResult(Result.Success());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return Task.FromResult(Result.Failure($"Error deleting file: {ex.Message}", StatusCodes.Status500InternalServerError));
            }
        }
    }
}
