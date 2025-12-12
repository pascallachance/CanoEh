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

        public async Task<Result<string>> UploadFileAsync(IFormFile file, string? fileName = null, string? subPath = null)
        {
            try
            {
                _logger.LogInformation("=== UploadFileAsync START ===");
                _logger.LogInformation("ContentRootPath: {ContentRootPath}", _contentRootPath);
                _logger.LogInformation("Input - FileName: {FileName}, SubPath: {SubPath}, FileLength: {FileLength}", 
                    fileName ?? "null", subPath ?? "null", file?.Length ?? 0);
                
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("File is empty or not provided");
                    return Result.Failure<string>("File is empty or not provided.", StatusCodes.Status400BadRequest);
                }

                // Validate file type (images only)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Result.Failure<string>($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}", StatusCodes.Status400BadRequest);
                }

                // Verify MIME type matches the file extension
                var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!string.IsNullOrEmpty(file.ContentType) && !allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    return Result.Failure<string>("Invalid file content type.", StatusCodes.Status400BadRequest);
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

                // Validate fileName to prevent path traversal
                if (fileName != Path.GetFileName(fileName) ||
                    fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return Result.Failure<string>("Invalid file name.", StatusCodes.Status400BadRequest);
                }

                // Validate and sanitize subPath if provided
                if (!string.IsNullOrWhiteSpace(subPath))
                {
                    // Normalize path separators
                    subPath = subPath.Replace('\\', '/');
                    
                    // Validate subPath to prevent path traversal
                    if (subPath.Contains("..") || 
                        subPath.StartsWith('/') ||
                        subPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                    {
                        return Result.Failure<string>("Invalid subpath.", StatusCodes.Status400BadRequest);
                    }
                }

                // Build the full directory path
                var uploadsPath = Path.Combine(_contentRootPath, "wwwroot", _uploadFolder);
                _logger.LogInformation("Base uploads path: {UploadsPath}", uploadsPath);
                
                if (!string.IsNullOrWhiteSpace(subPath))
                {
                    uploadsPath = Path.Combine(uploadsPath, subPath);
                    _logger.LogInformation("Full uploads path with subPath: {UploadsPath}", uploadsPath);
                }

                // Ensure the full directory path (including subdirectories) exists
                // Directory.CreateDirectory creates all directories and subdirectories in the path
                // It does not throw an exception if the directory already exists
                // We check existence first to provide better logging (distinguish "created" vs "already exists")
                _logger.LogInformation("Checking if directory exists: {Path}", uploadsPath);
                if (!Directory.Exists(uploadsPath))
                {
                    _logger.LogInformation("Directory does not exist. Creating directory at {Path}", uploadsPath);
                    var createdDir = Directory.CreateDirectory(uploadsPath);
                    _logger.LogInformation("Successfully created directory at {Path}. Full path: {FullPath}", uploadsPath, createdDir.FullName);
                }
                else
                {
                    _logger.LogInformation("Directory already exists at {Path}", uploadsPath);
                }

                // Save the file
                var filePath = Path.Combine(uploadsPath, fileName);
                
                _logger.LogInformation("Attempting to save file to {FilePath}", filePath);
                _logger.LogInformation("File path details - Directory: {Directory}, FileName: {FileName}, Full path exists: {DirectoryExists}", 
                    Path.GetDirectoryName(filePath), Path.GetFileName(filePath), Directory.Exists(Path.GetDirectoryName(filePath)));
                
                // Overwrite existing file if it exists (allows updating images)
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    _logger.LogInformation("FileStream created, copying file data...");
                    await file.CopyToAsync(stream);
                    await stream.FlushAsync();
                    _logger.LogInformation("File data copied and flushed to stream. Stream position: {Position}, length: {Length}", stream.Position, stream.Length);
                }

                // Verify file was created and get file info
                _logger.LogInformation("Verifying file creation at {FilePath}", filePath);
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    _logger.LogError("File was not created at expected location: {FilePath}", filePath);
                    _logger.LogError("Directory exists: {DirectoryExists}, Directory contents: {Contents}", 
                        Directory.Exists(Path.GetDirectoryName(filePath)),
                        Directory.Exists(Path.GetDirectoryName(filePath)) ? string.Join(", ", Directory.GetFiles(Path.GetDirectoryName(filePath) ?? "")) : "N/A");
                    return Result.Failure<string>("File upload failed - file not created on disk.", StatusCodes.Status500InternalServerError);
                }

                _logger.LogInformation("File saved successfully: {FilePath} (Size: {Size} bytes)", filePath, fileInfo.Length);
                _logger.LogInformation("File attributes - CreationTime: {CreationTime}, LastWriteTime: {LastWriteTime}", fileInfo.CreationTime, fileInfo.LastWriteTime);

                // Build the relative path for URL
                var relativePath = string.IsNullOrWhiteSpace(subPath) 
                    ? fileName 
                    : $"{subPath}/{fileName}";

                _logger.LogInformation("File uploaded successfully: {RelativePath}", relativePath);

                // Return the URL
                var fileUrl = GetFileUrl(relativePath);
                _logger.LogInformation("Generated file URL: {FileUrl}", fileUrl);
                _logger.LogInformation("=== UploadFileAsync SUCCESS ===");
                return Result.Success(fileUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== UploadFileAsync FAILED === Error uploading file: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                return Result.Failure<string>($"Error uploading file: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        public string GetFileUrl(string filePath)
        {
            // Normalize path separators for URL
            filePath = filePath.Replace('\\', '/');
            
            // Return a relative URL that can be used by the frontend
            return $"/{_uploadFolder}/{filePath}";
        }

        public Task<Result> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return Task.FromResult(Result.Failure("File path is required.", StatusCodes.Status400BadRequest));
                }

                // Normalize path separators
                filePath = filePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                // Prevent path traversal: reject file paths with relative components
                if (filePath.Contains(".."))
                {
                    return Task.FromResult(Result.Failure("Invalid file path.", StatusCodes.Status400BadRequest));
                }

                var uploadsRoot = Path.GetFullPath(Path.Combine(_contentRootPath, "wwwroot", _uploadFolder));
                var fullFilePath = Path.GetFullPath(Path.Combine(uploadsRoot, filePath));

                // Ensure the file is within the uploads directory
                if (!fullFilePath.StartsWith(uploadsRoot + Path.DirectorySeparatorChar) && fullFilePath != uploadsRoot)
                {
                    return Task.FromResult(Result.Failure("Invalid file path.", StatusCodes.Status400BadRequest));
                }
                
                if (!File.Exists(fullFilePath))
                {
                    return Task.FromResult(Result.Failure("File not found.", StatusCodes.Status404NotFound));
                }

                File.Delete(fullFilePath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);

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
