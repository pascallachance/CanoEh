# Image Storage Structure Documentation

## Overview

This document describes the hierarchical image storage structure implemented for CanoEh e-commerce application. The structure organizes uploaded images by company and item variant, making it easy to manage and migrate to cloud storage services like Azure Blob Storage.

## Folder Structure

### Item Variant Images

Item variant images (thumbnails and additional images) are stored using the following structure:

```
wwwroot/uploads/
  └── {companyID}/
      └── {ItemVariantID}/
          ├── {ItemVariantID}_thumb.jpg      # Thumbnail image
          ├── {ItemVariantID}_1.jpg          # First additional image
          ├── {ItemVariantID}_2.jpg          # Second additional image
          └── {ItemVariantID}_3.jpg          # Third additional image
```

**Example:**
```
wwwroot/uploads/
  └── a1b2c3d4-e5f6-7890-abcd-ef1234567890/
      └── f1e2d3c4-b5a6-7890-cdef-123456789abc/
          ├── f1e2d3c4-b5a6-7890-cdef-123456789abc_thumb.jpg
          ├── f1e2d3c4-b5a6-7890-cdef-123456789abc_1.jpg
          └── f1e2d3c4-b5a6-7890-cdef-123456789abc_2.jpg
```

### Company Logos

Company logos are stored directly under the company folder:

```
wwwroot/uploads/
  └── {companyID}/
      └── {companyID}_logo.jpg               # Company logo
```

**Example:**
```
wwwroot/uploads/
  └── a1b2c3d4-e5f6-7890-abcd-ef1234567890/
      └── a1b2c3d4-e5f6-7890-abcd-ef1234567890_logo.jpg
```

## API Endpoints

### 1. Upload Item Variant Image

**Endpoint:** `POST /api/Item/UploadImage`

**Authentication:** Required (JWT Bearer token)

**Query Parameters:**
- `variantId` (Guid, required): The ID of the item variant
- `imageType` (string, optional): Either "thumbnail" or "image" (default: "image")
- `imageNumber` (int, optional): Image number for additional images (default: 1)

**Request Body:** 
- Form-data with key `file` containing the image file

**Response:**
```json
{
  "imageUrl": "/uploads/{companyID}/{variantID}/{variantID}_thumb.jpg"
}
```

**Example Usage:**

```bash
# Upload thumbnail
curl -X POST "https://localhost:5199/api/Item/UploadImage?variantId=f1e2d3c4-b5a6-7890-cdef-123456789abc&imageType=thumbnail" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@thumbnail.jpg"

# Upload first additional image
curl -X POST "https://localhost:5199/api/Item/UploadImage?variantId=f1e2d3c4-b5a6-7890-cdef-123456789abc&imageType=image&imageNumber=1" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@image1.jpg"
```

**Business Logic:**
- Validates that the authenticated user owns the item (via SellerID)
- Creates subdirectories automatically if they don't exist
- Uses SellerID as the companyID in the folder structure

### 2. Upload Company Logo

**Endpoint:** `POST /api/Company/UploadLogo`

**Authentication:** Required (JWT Bearer token)

**Query Parameters:**
- `companyId` (Guid, required): The ID of the company

**Request Body:** 
- Form-data with key `file` containing the logo image file

**Response:**
```json
{
  "logoUrl": "/uploads/{companyID}/{companyID}_logo.jpg"
}
```

**Example Usage:**

```bash
curl -X POST "https://localhost:5199/api/Company/UploadLogo?companyId=a1b2c3d4-e5f6-7890-abcd-ef1234567890" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@logo.jpg"
```

**Business Logic:**
- Validates that the authenticated user owns the company (via OwnerID)
- Creates the company directory automatically if it doesn't exist

## Implementation Details

### IFileStorageService Interface

```csharp
public interface IFileStorageService
{
    Task<Result<string>> UploadFileAsync(IFormFile file, string? fileName = null, string? subPath = null);
    string GetFileUrl(string filePath);
    Task<Result> DeleteFileAsync(string filePath);
}
```

**Key Changes:**
- Added `subPath` parameter to `UploadFileAsync` for subdirectory support
- `GetFileUrl` and `DeleteFileAsync` now accept full relative paths instead of just file names

### LocalFileStorageService Implementation

**Features:**
1. **Automatic Directory Creation:** Creates subdirectories as needed
2. **Path Traversal Prevention:** Validates paths to prevent security vulnerabilities
3. **Hierarchical Path Support:** Handles nested folder structures
4. **Consistent URL Generation:** Returns URLs that work with both local and cloud storage

**Path Validation:**
- Rejects paths containing ".." (parent directory references)
- Rejects paths starting with "/" or "\\" (absolute paths)
- Validates against invalid path characters
- Ensures all file operations stay within the uploads directory

## Security Features

### Authentication & Authorization
- All upload endpoints require valid JWT authentication
- Users can only upload images for items/companies they own
- Ownership is verified before any file operations

### File Validation
- **File Types:** Only image files (JPG, JPEG, PNG, GIF, WebP)
- **File Size:** Maximum 5MB per file
- **MIME Type:** Validated against allowed image types
- **Path Security:** Prevents path traversal attacks

### Error Handling
- Clear error messages for validation failures
- Appropriate HTTP status codes (400, 401, 403, 404, 409, 500)
- Logging of all file operations

## Migration to Cloud Storage

The current implementation is designed to support easy migration to cloud storage (e.g., Azure Blob Storage):

### Benefits of Current Structure
1. **Logical Organization:** Files are grouped by company and variant
2. **Scalable:** Hierarchical structure works well with cloud storage blob naming
3. **Consistent Naming:** File names follow predictable patterns
4. **Easy Migration:** The `subPath` parameter maps directly to blob container paths

### Future Azure Blob Storage Implementation

When migrating to Azure Blob Storage, the interface remains the same:

```csharp
public class AzureBlobStorageService : IFileStorageService
{
    public async Task<Result<string>> UploadFileAsync(IFormFile file, string? fileName = null, string? subPath = null)
    {
        // subPath maps directly to blob path:
        // subPath: "companyId/variantId" → blob path: "uploads/companyId/variantId/filename.jpg"
        var blobPath = string.IsNullOrEmpty(subPath) 
            ? $"uploads/{fileName}" 
            : $"uploads/{subPath}/{fileName}";
        
        // Upload to Azure Blob Storage...
    }
}
```

### Migration Steps
1. Create Azure Blob Storage container named "uploads"
2. Implement `AzureBlobStorageService` class
3. Copy existing files from local storage to Azure Blob Storage
4. Update DI registration in `Program.cs` to use Azure service
5. No changes needed to controllers or business logic

## Testing

### Unit Tests
Location: `/API.Tests/LocalFileStorageServiceShould.cs`

**Test Coverage:**
- ✅ Upload file with subdirectory path
- ✅ Create subdirectories automatically
- ✅ Prevent path traversal attacks
- ✅ Generate correct URLs for hierarchical paths
- ✅ Delete files from subdirectories
- ✅ Follow hierarchical structure for item variants
- ✅ Follow hierarchical structure for company logos

### Manual Testing
See `/tmp/test-image-upload.md` for detailed manual testing procedures.

## Best Practices

### For Frontend Developers
1. **Always specify imageType:** Use "thumbnail" for thumbnails and "image" for additional images
2. **Use sequential imageNumbers:** Start from 1 and increment (1, 2, 3, etc.)
3. **Handle errors gracefully:** Check response status codes and display appropriate messages
4. **Show upload progress:** Image uploads can take time, show progress indicators

### For Backend Developers
1. **Don't bypass the service:** Always use `IFileStorageService` for file operations
2. **Validate ownership:** Always check user permissions before allowing uploads
3. **Log operations:** Log all successful and failed file operations for debugging
4. **Handle cleanup:** Delete old images when updating to avoid storage bloat

## Troubleshooting

### Common Issues

**Issue:** "Invalid file path" error
- **Cause:** Path contains "..", starts with "/", or has invalid characters
- **Solution:** Ensure subPath parameter only contains valid directory names separated by "/"

**Issue:** "A file with this name already exists"
- **Cause:** Trying to upload a file that already exists
- **Solution:** Delete the old file first or use a different image number

**Issue:** "Variant not found or you do not have permission"
- **Cause:** User doesn't own the item or variant doesn't exist
- **Solution:** Verify the variantId belongs to an item owned by the authenticated user

**Issue:** Files not accessible via URL
- **Cause:** Static file middleware not configured correctly
- **Solution:** Ensure `app.UseStaticFiles()` is called in Program.cs

## Performance Considerations

### Local Storage
- **Directory Creation:** O(1) for existing directories, minimal overhead for new directories
- **File Write:** Standard filesystem performance
- **Concurrent Uploads:** Safe due to unique file names per variant

### Future Cloud Storage
- **Throughput:** Cloud storage provides better throughput for concurrent uploads
- **CDN Integration:** Easy to add CDN in front of blob storage for faster delivery
- **Global Distribution:** Files can be replicated across regions

## Monitoring and Maintenance

### Recommended Monitoring
1. **Upload Success Rate:** Track successful vs failed uploads
2. **Storage Usage:** Monitor disk space or blob storage quota
3. **Average Upload Time:** Track performance over time
4. **Error Types:** Monitor common error patterns

### Maintenance Tasks
1. **Cleanup Orphaned Files:** Periodically remove files for deleted items/variants
2. **Backup:** Regular backups of uploaded files
3. **Storage Optimization:** Compress or resize images if needed
4. **Security Audits:** Regular review of file access logs

## Related Documentation
- [API Documentation](../README.md)
- [Authentication Guide](./SESSION_API_DOCUMENTATION.md)
- [Item Creation Flow](./item-creation-flow.md)
