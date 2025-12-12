# Image Upload Directory Creation - Implementation Details

## Overview

This document explains how directory creation works during image uploads in the CanoEh application and describes the enhancements made to improve diagnostics and verification.

## Problem Statement

Users reported uncertainty about whether directories are automatically created when uploading product variant images via the `/api/Item/UploadImage` endpoint. Specifically:
- Images for product variants should be stored in `API/wwwroot/uploads/{companyId}/{variantId}/`
- The question was: "If directories don't exist, are they created?"

## Solution

**Yes, directories ARE automatically created.** This has always been the case, but we've enhanced the logging and verification to make this more transparent.

### How Directory Creation Works

The `LocalFileStorageService` uses .NET's `Directory.CreateDirectory()` method, which:

1. **Creates all intermediate directories**: If you request path `wwwroot/uploads/company-guid/variant-guid/`, it will create all folders in that path
2. **Handles existing directories gracefully**: Does NOT throw an exception if directories already exist
3. **Is atomic**: The operation is thread-safe for concurrent uploads

### Code Implementation

From `Infrastructure/Services/LocalFileStorageService.cs`:

```csharp
// Build the full directory path
var uploadsPath = Path.Combine(_contentRootPath, "wwwroot", _uploadFolder);
if (!string.IsNullOrWhiteSpace(subPath))
{
    uploadsPath = Path.Combine(uploadsPath, subPath);
}

// Ensure the full directory path (including subdirectories) exists
// Directory.CreateDirectory creates all directories and subdirectories in the path
// It does not throw an exception if the directory already exists
// We check existence first to provide better logging (distinguish "created" vs "already exists")
if (!Directory.Exists(uploadsPath))
{
    _logger.LogInformation("Creating directory at {Path}", uploadsPath);
    Directory.CreateDirectory(uploadsPath);
    _logger.LogInformation("Successfully created directory at {Path}", uploadsPath);
}
else
{
    _logger.LogDebug("Directory already exists at {Path}", uploadsPath);
}
```

## Enhancements Made

### 1. Enhanced Logging

The file upload process now logs the following events:

- **Before directory creation**: `"Creating directory at {Path}"`
- **After successful creation**: `"Successfully created directory at {Path}"`
- **When directory exists**: `"Directory already exists at {Path}"` (debug level)
- **Before file save**: `"Attempting to save file to {FilePath}"`
- **After successful save**: `"File saved successfully: {FilePath} (Size: {Size} bytes)"`
- **Generated URL**: `"Generated file URL: {FileUrl}"`

### 2. File Verification

After saving a file, the service now:
1. Verifies the file actually exists on disk using `FileInfo.Exists`
2. Logs the file size for verification
3. Returns an explicit error if the file was not created

### 3. Better Error Messages

If file creation fails, you'll now see:
```
"File upload failed - file not created on disk."
```

This helps distinguish between:
- Network/API errors
- File system permission errors
- Storage space issues

## How to Verify Directory Creation

### Method 1: Check Application Logs

When you upload an image, you should see log entries like:

```
[Information] Creating directory at /path/to/CanoEh/API/wwwroot/uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890/f1e2d3c4-b5a6-7890-cdef-123456789abc
[Information] Successfully created directory at /path/to/CanoEh/API/wwwroot/uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890/f1e2d3c4-b5a6-7890-cdef-123456789abc
[Information] Attempting to save file to /path/to/CanoEh/API/wwwroot/uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890/f1e2d3c4-b5a6-7890-cdef-123456789abc/f1e2d3c4-b5a6-7890-cdef-123456789abc_thumb.jpg
[Information] File saved successfully: /path/to/CanoEh/API/wwwroot/uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890/f1e2d3c4-b5a6-7890-cdef-123456789abc/f1e2d3c4-b5a6-7890-cdef-123456789abc_thumb.jpg (Size: 45632 bytes)
[Information] File uploaded successfully: a1b2c3d4-e5f6-7890-abcd-ef1234567890/f1e2d3c4-b5a6-7890-cdef-123456789abc/f1e2d3c4-b5a6-7890-cdef-123456789abc_thumb.jpg
[Information] Generated file URL: /uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890/f1e2d3c4-b5a6-7890-cdef-123456789abc/f1e2d3c4-b5a6-7890-cdef-123456789abc_thumb.jpg
```

### Method 2: Check File System

After uploading an image:

1. Navigate to your project's API directory
2. Look in `API/wwwroot/uploads/`
3. You should see folders structured like:
   ```
   uploads/
   └── {company-guid}/
       └── {variant-guid}/
           ├── {variant-guid}_thumb.jpg
           ├── {variant-guid}_1.jpg
           └── {variant-guid}_2.jpg
   ```

### Method 3: API Response

A successful upload returns:
```json
{
  "imageUrl": "/uploads/{companyId}/{variantId}/{filename}.jpg"
}
```

If directories couldn't be created, you'd get an error instead.

## Testing

### Automated Tests

The functionality is tested in `API.Tests/LocalFileStorageServiceShould.cs`:

- ✅ `UploadFile_CreateSubdirectories_WhenSubPathProvided` - Verifies nested directories are created
- ✅ `UploadFile_FollowHierarchicalStructure_ForItemVariant` - Tests the exact structure used for product variants
- ✅ `UploadFile_VerifyFileExistsOnDisk_AfterSuccessfulUpload` - Confirms files actually exist on disk

All 17 tests pass successfully.

### Manual Testing

To manually test:

1. Start the API server:
   ```bash
   cd API
   dotnet run
   ```

2. Upload an image via Swagger UI or curl:
   ```bash
   curl -X POST "http://localhost:5269/api/Item/UploadImage?variantId={your-variant-guid}&imageType=thumbnail" \
     -H "Authorization: Bearer {your-token}" \
     -F "file=@/path/to/test-image.jpg"
   ```

3. Check the logs and file system as described above

## Common Issues and Solutions

### Issue: "File upload failed - file not created on disk"

**Possible Causes:**
- Insufficient disk space
- Permission issues (application can't write to wwwroot/uploads)
- Antivirus blocking file creation

**Solution:**
1. Check disk space: `df -h` (Linux/Mac) or check drive properties (Windows)
2. Check permissions: Ensure the application has write access to `API/wwwroot/uploads`
3. Check application logs for more specific error messages

### Issue: Directories not appearing in expected location

**Possible Causes:**
- Looking in wrong directory (Store.Server vs API)
- Application running from different location than expected

**Solution:**
1. Check logs for actual path being used
2. Verify you're looking in the correct project directory
3. The path is based on `ContentRootPath` which is set in Program.cs

## Performance Considerations

### Directory Existence Check

The code checks `Directory.Exists()` before calling `Directory.CreateDirectory()`. While technically redundant (CreateDirectory handles existing directories), this check is intentionally kept for two reasons:

1. **Better logging**: Distinguish between "created" vs "already existed"
2. **Minimal performance impact**: For file uploads that involve writing megabytes of data, one extra file system check is negligible

### File Verification

After saving, the code uses `FileInfo.Exists` to verify the file was created and get its size. This is efficient as it only makes one file system call.

## Related Documentation

- [IMAGE_STORAGE_STRUCTURE.md](./IMAGE_STORAGE_STRUCTURE.md) - Complete guide to image storage structure
- [Microsoft Docs: Directory.CreateDirectory](https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.createdirectory)

## Summary

✅ **Directories ARE automatically created** when uploading images
✅ **All intermediate directories** in the path are created
✅ **Enhanced logging** makes this behavior transparent
✅ **File verification** ensures uploads complete successfully
✅ **Comprehensive tests** validate the functionality

If you have any questions or encounter issues, check the application logs first - they now provide detailed information about every step of the upload process.
