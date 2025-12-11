using Helpers.Common;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Uploads a file to the storage system.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="fileName">Optional custom file name. If not provided, a unique name will be generated.</param>
        /// <param name="subPath">Optional subdirectory path (e.g., "companyId/variantId"). Directories will be created automatically.</param>
        /// <returns>A Result containing the file URL if successful, or an error message.</returns>
        Task<Result<string>> UploadFileAsync(IFormFile file, string? fileName = null, string? subPath = null);

        /// <summary>
        /// Gets the URL for accessing a file.
        /// </summary>
        /// <param name="filePath">The relative path to the file (can include subdirectories).</param>
        /// <returns>The URL for accessing the file.</returns>
        string GetFileUrl(string filePath);

        /// <summary>
        /// Deletes a file from the storage system.
        /// </summary>
        /// <param name="filePath">The relative path to the file to delete (can include subdirectories).</param>
        /// <returns>A Result indicating success or failure.</returns>
        Task<Result> DeleteFileAsync(string filePath);
    }
}
