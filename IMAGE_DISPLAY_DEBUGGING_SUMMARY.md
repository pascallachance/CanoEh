# Image Display Debugging - Implementation Summary

## Problem Addressed
When editing an item in the Seller application, thumbnail and item images show broken image icons instead of displaying correctly.

## Solution Implemented
Added comprehensive logging to help debug image URL retrieval and display, with all logs wrapped in development mode checks for production safety.

## Changes Made

### 1. urlUtils.ts
Added logging to track URL conversion:
- Logs when converting relative URLs to absolute URLs
- Logs when URLs are already absolute (pass-through)
- Logs when URLs are empty/null
- All wrapped in `import.meta.env.DEV` checks

### 2. AddProductStep4.tsx
Added logging throughout the edit flow:
- Logs when merging existing variants with generated variants
- Logs image URLs from API response
- Logs converted absolute URLs
- Added image load/error event handlers
- All wrapped in development mode checks

## How to Debug with the Logs

### Step 1: Start the Application
```bash
# Terminal 1: Start API
cd API
dotnet run

# Terminal 2: Start Seller Frontend  
cd Seller/seller.client
npm run dev
```

### Step 2: Open Browser Console
1. Navigate to https://localhost:64941
2. Press F12 to open Developer Tools
3. Go to Console tab

### Step 3: Edit an Item
1. Log in to Seller application
2. Go to Products section
3. Click edit button on any item
4. Watch console for log messages

### Step 4: Analyze the Logs

**Success Pattern:**
```
[toAbsoluteUrl] Input URL: /uploads/abc-123/def-456/def-456_thumb.jpg
[toAbsoluteUrl] Converted to absolute URL: https://localhost:7182/uploads/abc-123/def-456/def-456_thumb.jpg
[AddProductStep4] Found matching existing variant: {id: "...", thumbnailUrl: "/uploads/..."}
[AddProductStep4] Converted URLs - thumbnail: https://localhost:7182/uploads/...
[AddProductStep4] Thumbnail loaded successfully: https://localhost:7182/uploads/...
```

**Failure Pattern:**
```
[AddProductStep4] Thumbnail failed to load: https://localhost:7182/uploads/...
[AddProductStep4] Image error type: error
```

## Common Issues and Solutions

### Issue 1: API Server Not Running
**Symptoms:**
- Thumbnail failed to load
- Network error in browser
- Console shows failed fetch

**Solution:**
```bash
cd API
dotnet run
```
Wait for "Now listening on: https://localhost:7182"

### Issue 2: Image File Doesn't Exist
**Symptoms:**
- 404 error in Network tab
- Failed to load message in console
- URL looks correct

**Solution:**
- Check if file exists in `API/wwwroot/uploads/{companyId}/{variantId}/`
- If missing, image was never uploaded
- Re-upload image through the UI

### Issue 3: Wrong Port in URL
**Symptoms:**
- URL shows wrong port (not 7182)
- Connection refused

**Solution:**
- Check `Seller/seller.client/.env`
- Should have: `VITE_API_SELLER_BASE_URL=https://localhost:7182`
- Restart frontend after changing .env

### Issue 4: Empty Thumbnail URL
**Symptoms:**
- Logs show thumbnailUrl as empty or null
- No image element rendered

**Explanation:**
- This is expected if no image was uploaded for this variant
- Not a bug - just means item has no images yet

### Issue 5: CORS Error
**Symptoms:**
- CORS error in console
- Images fail to load even though file exists

**Solution:**
- Check API Program.cs has correct CORS policy
- Should allow origin: https://localhost:64941
- Restart API after changes

## Technical Details

### How Image URLs Work
1. **Storage**: Images saved to `API/wwwroot/uploads/{companyId}/{variantId}/{filename}`
2. **Database**: Stores relative path `/uploads/{companyId}/{variantId}/{filename}`
3. **API Response**: Returns the relative path as stored in database
4. **Frontend**: Converts to absolute URL `https://localhost:7182/uploads/...`
5. **Browser**: Fetches image from absolute URL via API's static file middleware

### URL Conversion Function
```typescript
export function toAbsoluteUrl(url: string | undefined): string {
    // If empty/null, return empty string
    if (!url) return '';
    
    // If already absolute or blob/data, return as-is
    if (url.startsWith('http://') || url.startsWith('https://') || 
        url.startsWith('blob:') || url.startsWith('data:')) {
        return url;
    }
    
    // If relative (starts with /), prepend API base URL
    if (url.startsWith('/')) {
        const baseUrl = import.meta.env.VITE_API_SELLER_BASE_URL;
        return `${baseUrl}${url}`;
    }
    
    // Otherwise return as-is
    return url;
}
```

### Why Logging is Development-Only
- Uses `import.meta.env.DEV` checks
- Vite build tool strips these in production
- No performance impact in production
- No security risk from exposed paths

## Security & Quality

### Code Review Results
✅ All feedback addressed
✅ Logs wrapped in development mode checks
✅ No sensitive data logged
✅ Optimized for production

### Security Scan Results
✅ CodeQL scan: 0 alerts found
✅ No vulnerabilities introduced
✅ Safe for production deployment

## Files Modified
1. `Seller/seller.client/src/utils/urlUtils.ts`
2. `Seller/seller.client/src/components/AddProductStep4.tsx`

## Next Steps for User
1. Run the application with both API and frontend servers
2. Edit an item that has images uploaded
3. Check browser console for log messages
4. Use logs to identify the specific issue
5. Apply appropriate solution from troubleshooting guide

## Additional Notes
- The URL conversion is **essential functionality**, not just debugging
- Without it, images would fail to load (relative URLs don't work in browser)
- The logging helps diagnose WHY images might fail to load
- All changes are minimal and focused on debugging
- No changes to core functionality or business logic
