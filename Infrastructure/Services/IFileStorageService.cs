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
        /// <returns>A Result containing the file URL if successful, or an error message.</returns>
        Task<Result<string>> UploadFileAsync(IFormFile file, string? fileName = null);

        /// <summary>
        /// Gets the URL for accessing a file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The URL for accessing the file.</returns>
        string GetFileUrl(string fileName);

        /// <summary>
        /// Deletes a file from the storage system.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>A Result indicating success or failure.</returns>
        Task<Result> DeleteFileAsync(string fileName);
    }
}
