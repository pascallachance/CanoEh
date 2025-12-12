# Image Upload Troubleshooting Guide

## Issue Description
Images and thumbnails for ItemVariant are not being saved to the expected folder structure:
- Expected: `API\wwwroot\uploads\{CompanyID}\{ItemVariantID}\`
- Observed: The `\API\wwwroot\uploads` folder appears empty (contains only `.gitkeep` file)

## Investigation Results ✅

**Good news!** The image upload implementation has been thoroughly tested and is **working correctly**:

### Test Results
All integration tests pass (4/4):
1. ✅ Directory structure `{CompanyID}/{ItemVariantID}/` is created properly
2. ✅ Files are saved with correct names: `{variantId}_thumb.jpg`, `{variantId}_1.jpg`, etc.
3. ✅ Nested subdirectories are created automatically by `Directory.CreateDirectory()`
4. ✅ File content is preserved correctly
5. ✅ Error handling works for invalid variants

Run tests yourself:
```bash
dotnet test --filter "FullyQualifiedName~ImageUploadIntegrationShould"
dotnet test --filter "FullyQualifiedName~LocalFileStorageServiceShould"
```

## Enhanced Logging 📝

Comprehensive logging has been added to help diagnose issues:

### LocalFileStorageService Logs
- ContentRootPath at start
- Input parameters (fileName, subPath, fileLength)
- Base uploads path and full path with subPath
- Directory existence checks and creation attempts
- File path details before writing
- File stream operations and flush
- File verification with directory contents
- File attributes after creation (size, timestamps)
- Stack traces on errors

### ItemController.UploadImage Logs
- All request parameters (variantId, imageType, imageNumber)
- File information (name, size, content type)
- Authentication details (user claims, userId)
- Item retrieval process
- Upload parameters (subPath, fileName)
- FileStorageService results
- Stack traces on errors

### Viewing Logs
Logs will appear in:
- Console output when running `dotnet run`
- Visual Studio Output window
- Application Insights (if configured)
- Log files (if file logging is configured)

## Common Issues & Solutions 🔍

### 1. API Endpoint Not Being Called
**Symptom:** No logs appear, uploads folder remains empty

**Causes:**
- API server not running
- Wrong API URL being called
- CORS issues preventing requests

**Solution:**
```bash
# Start the API
cd API
dotnet run

# Verify API is running
curl http://localhost:5269/swagger/index.html
```

### 2. Authentication Failure
**Symptom:** Logs show "User ID not found in token" or 401 Unauthorized

**Causes:**
- Not authenticated (no JWT token)
- Token expired
- Token not sent in request

**Solution:**
1. Obtain a valid JWT token by logging in first
2. Include token in Authorization header: `Bearer {token}`
3. Check token expiration time

### 3. Variant Not Found
**Symptom:** Logs show "Variant not found or you do not have permission"

**Causes:**
- Invalid variantId provided
- User is not the owner (SellerID doesn't match userId)
- Variant has been soft-deleted

**Solution:**
1. Verify the variantId exists in database
2. Check that `Item.SellerID` matches the authenticated user's ID
3. Verify variant is not soft-deleted (`Deleted = 0`)

### 4. File System Permissions (Windows)
**Symptom:** Logs show "Access denied" or "Permission denied" errors

**Causes:**
- API process doesn't have write permissions to `wwwroot\uploads`
- Antivirus blocking file writes
- Directory is read-only

**Solution:**
1. Run as Administrator (temporary test only)
2. Check folder permissions: Right-click → Properties → Security
3. Grant "Full Control" to user running the API
4. Check antivirus logs and add exception if needed

### 5. Wrong ContentRootPath
**Symptom:** Files are saved but in unexpected location

**Causes:**
- API running from different directory than expected
- Multiple instances of the project

**Solution:**
1. Check logs for `ContentRootPath` value
2. Files will be in: `{ContentRootPath}\wwwroot\uploads\{CompanyID}\{ItemVariantID}\`
3. Search for files: `Get-ChildItem -Path "C:\Users\lacha\source\repos\CanoEh" -Filter "*_thumb.jpg" -Recurse`

## Testing the Upload Endpoint 🧪

### Prerequisites
1. Create a user and login to get JWT token
2. Create an item with a variant
3. Note the variantId and ensure SellerID matches your userId

### Using Swagger UI
1. Navigate to `http://localhost:5269/swagger`
2. Click **Authorize** and enter JWT token: `Bearer {your-token}`
3. Find `/api/Item/UploadImage` endpoint
4. Fill in parameters:
   - `file`: Select an image file
   - `variantId`: Your variant's GUID
   - `imageType`: `thumbnail` or `image`
   - `imageNumber`: `1` (for additional images)
5. Click **Execute**
6. Check Response body for `imageUrl`

### Using curl
```bash
# Upload thumbnail
curl -X POST "http://localhost:5269/api/Item/UploadImage?variantId={VARIANT-ID}&imageType=thumbnail" \
  -H "Authorization: Bearer {YOUR-JWT-TOKEN}" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/image.jpg"

# Upload additional image
curl -X POST "http://localhost:5269/api/Item/UploadImage?variantId={VARIANT-ID}&imageType=image&imageNumber=1" \
  -H "Authorization: Bearer {YOUR-JWT-TOKEN}" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/image2.jpg"
```

### Using PowerShell
```powershell
$token = "your-jwt-token"
$variantId = "your-variant-guid"
$imagePath = "C:\path\to\image.jpg"

$headers = @{
    "Authorization" = "Bearer $token"
}

$form = @{
    file = Get-Item -Path $imagePath
}

Invoke-RestMethod -Uri "http://localhost:5269/api/Item/UploadImage?variantId=$variantId&imageType=thumbnail" `
    -Method Post `
    -Headers $headers `
    -Form $form
```

## Verifying Upload Success ✅

### 1. Check Response
Successful upload returns:
```json
{
  "imageUrl": "/uploads/{CompanyID}/{VariantID}/{VariantID}_thumb.jpg"
}
```

### 2. Check File System
```powershell
# PowerShell
Get-ChildItem -Path "C:\Users\lacha\source\repos\CanoEh\API\wwwroot\uploads" -Recurse

# Expected structure:
# wwwroot/
#   uploads/
#     {CompanyID}/
#       {VariantID}/
#         {VariantID}_thumb.jpg
#         {VariantID}_1.jpg
#         {VariantID}_2.jpg
```

### 3. Check Logs
Look for these log entries:
- ✅ `=== UploadImage API START ===`
- ✅ `ContentRootPath: ...`
- ✅ `Creating directory at ...` or `Directory already exists at ...`
- ✅ `File saved successfully: ... (Size: ... bytes)`
- ✅ `=== UploadImage API SUCCESS ===`

## Expected Directory Structure

```
API/
└── wwwroot/
    └── uploads/
        └── {CompanyID (Guid)}/
            └── {ItemVariantID (Guid)}/
                ├── {ItemVariantID}_thumb.jpg    (thumbnail)
                ├── {ItemVariantID}_1.jpg         (additional image 1)
                ├── {ItemVariantID}_2.jpg         (additional image 2)
                └── {ItemVariantID}_3.jpg         (additional image 3)
```

## API Endpoint Details

**Endpoint:** `POST /api/Item/UploadImage`

**Authorization:** Required (JWT Bearer token)

**Parameters:**
- `file` (form-data, required): Image file to upload
- `variantId` (query, required): GUID of the item variant
- `imageType` (query, optional): `"thumbnail"` or `"image"` (default: `"image"`)
- `imageNumber` (query, optional): Image number for additional images (default: `1`)

**Validations:**
- File must be an image: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- File size must be ≤ 5MB
- MIME type must match: `image/jpeg`, `image/png`, etc.
- User must own the item (Item.SellerID must match authenticated userId)
- Variant must exist and not be soft-deleted

**Returns:**
- `200 OK` with `{ "imageUrl": "/uploads/..." }`
- `400 Bad Request` if validation fails
- `401 Unauthorized` if not authenticated
- `404 Not Found` if variant doesn't exist or user doesn't own it
- `500 Internal Server Error` if upload fails

## Next Steps

1. **Run the API** and check console logs
2. **Test the upload endpoint** using Swagger, curl, or PowerShell
3. **Review the logs** to identify where the process fails
4. **Check the specific error** and apply the appropriate solution from above
5. **Verify file creation** in the expected directory

## Contact Support

If you continue to experience issues after following this guide:
1. Provide the full console logs from the API
2. Share the exact request you're sending (with sensitive data redacted)
3. Confirm your environment details (OS, .NET version, file permissions)
