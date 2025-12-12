# Thumbnail Display Fix - Testing Guide

## Problem Fixed
When editing an item, thumbnails were not displayed. Instead, a missing image icon appeared, even though the thumbnail files physically existed in `API/wwwroot/uploads/{SellerId}/{ItemVariantId}/`.

## Root Cause
The API returns thumbnail URLs as relative paths (e.g., `/uploads/sellerId/variantId/thumbnail.jpg`). When loading existing variants in edit mode, these relative URLs were used directly without being converted to absolute URLs by prepending the API base URL.

## Solution
Created utility functions to convert relative image URLs to absolute URLs when loading existing variant data in edit mode.

## Manual Testing Steps

### Prerequisites
1. .NET 8.0 SDK installed
2. Node.js v20+ installed
3. SQL Server LocalDB or SQLite configured

### Step 1: Start the Application

#### Option A: Using Seller Development Script (Recommended)
```bash
cd Seller
./start-dev.sh  # On Linux/Mac
# or
.\start-dev.ps1  # On Windows
```

This will:
- Build the .NET solution
- Install npm dependencies
- Start the API server on https://localhost:7182
- Start the Seller client on https://localhost:62209

#### Option B: Manual Start
```bash
# Terminal 1: Start API
cd API
dotnet run --launch-profile https

# Terminal 2: Start Seller Client
cd Seller/seller.client
npm install
npm run dev
```

### Step 2: Create a Product with Images

1. Open browser and navigate to https://localhost:62209
2. Click "Advanced" → "Proceed to localhost (unsafe)" if you see certificate warning
3. Log in with your seller credentials
4. Navigate to Products section
5. Click "Add Product"
6. Fill in product details:
   - **Step 1**: Enter product name and description (both English and French)
   - **Step 2**: Select a category
   - **Step 3**: Add variant attributes (e.g., Color: Red, Blue; Size: Small, Medium)
   - **Step 4**: For each variant:
     - Enter SKU (required)
     - Enter Price (required, must be > 0)
     - **Upload a thumbnail image** (this is what we're testing)
     - Optionally upload additional product images
7. Click "Create Product"
8. Verify product is created successfully

### Step 3: Test Thumbnail Display in Edit Mode

1. In the Products list, find the product you just created
2. Click the "Edit" button (pencil icon) for that product
3. You should see Step 1 with the product details
4. Click "Next" through steps 1, 2, and 3
5. On Step 4 (Configure Variants):
   - **VERIFY**: The thumbnail images you uploaded should be displayed for each variant
   - **EXPECTED**: You should see the actual thumbnail preview, not a missing image icon
   - **VERIFY**: The thumbnail previews should load correctly from the server

### Step 4: Test New Uploads Still Work

1. While still in edit mode on Step 4:
   - Choose a different thumbnail for one of the variants
   - **VERIFY**: The new thumbnail preview appears immediately
2. Click "Update Product"
3. **VERIFY**: Product updates successfully
4. Edit the product again
5. **VERIFY**: The newly uploaded thumbnail is displayed correctly

## Expected Results

### ✅ Before Fix (Issue)
- Thumbnails showed missing image icon when editing
- Console errors showing 404 for image requests like:
  ```
  GET /uploads/sellerId/variantId/thumbnail.jpg 404 (Not Found)
  ```

### ✅ After Fix (Working)
- Thumbnails display correctly when editing
- Image requests use absolute URLs like:
  ```
  GET https://localhost:7182/uploads/sellerId/variantId/thumbnail.jpg 200 (OK)
  ```

## Technical Details

### Files Changed
1. **`Seller/seller.client/src/utils/urlUtils.ts`** (NEW)
   - `toAbsoluteUrl()`: Converts relative URLs to absolute
   - `toAbsoluteUrlArray()`: Handles arrays and comma-separated URL strings

2. **`Seller/seller.client/src/components/AddProductStep4.tsx`** (MODIFIED)
   - Imports and uses the new utility functions
   - Converts thumbnail and image URLs when loading existing variant data

### URL Format Handling
The solution handles multiple URL formats:
- ✅ Absolute URLs (`http://` or `https://`): Returned unchanged
- ✅ Relative URLs (starts with `/`): Prepended with API base URL
- ✅ Blob URLs (`blob:`): Preserved for file preview functionality
- ✅ Data URLs (`data:`): Preserved for inline images
- ✅ Empty/undefined values: Filtered out appropriately

## Troubleshooting

### Thumbnails still not displaying?
1. Check browser console for errors
2. Verify API server is running on port 7182
3. Verify image files exist in `API/wwwroot/uploads/{SellerId}/{VariantId}/`
4. Check that CORS is properly configured
5. Verify `.env` file has correct `VITE_API_SELLER_BASE_URL=https://localhost:7182`

### Certificate warnings?
This is expected in development. The API uses a self-signed certificate. Click "Advanced" → "Proceed to localhost (unsafe)" to continue.

### Images not uploading?
1. Check file size (max 5MB per image)
2. Verify file format (allowed: jpg, jpeg, png, gif, webp)
3. Check browser console for upload errors
4. Verify user has write permissions to `API/wwwroot/uploads/`

## Additional Notes

- The fix is backward compatible and doesn't affect existing functionality
- No database schema changes required
- No API changes required
- The solution is reusable for other components that need URL conversion
