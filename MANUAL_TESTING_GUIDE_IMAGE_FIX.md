# Manual Testing Guide: Image Display Fix

## Prerequisites
1. .NET 8.0 SDK installed
2. Node.js 20+ installed
3. SQL Server LocalDB OR SQLite configured for the API
4. Product data in database with uploaded images

## Setup and Testing Steps

### 1. Start the API Server
```bash
cd /home/runner/work/CanoEh/CanoEh/API
dotnet run --launch-profile https
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7182
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5269
```

### 2. Verify API is Serving Static Files
```bash
# This should show static file middleware is enabled
curl -k -I https://localhost:7182/uploads/test.jpg
# Expected: 404 (file not found) BUT header should show it went through ASP.NET
```

### 3. Start the Store Frontend
```bash
cd /home/runner/work/CanoEh/CanoEh/Store/store.client
npm install  # If not already done
npm run dev
```

Expected output:
```
VITE v7.3.1  ready in XXXms
➜  Local:   https://localhost:64941/
```

### 4. Test Image Proxy Configuration
In a new terminal, test that the proxy forwards uploads requests:
```bash
# Test through Vite proxy - should forward to API
curl -k -I https://localhost:64941/uploads/test.jpg

# Compare with direct API request
curl -k -I https://localhost:7182/uploads/test.jpg

# Both should return similar headers (both go to API now)
```

### 5. Test in Browser
1. Open browser to `https://localhost:64941`
2. Accept the self-signed certificate warning
3. Open browser DevTools (F12)
4. Go to the Network tab
5. Filter by "uploads" or "img"
6. Look for the "Recently added items" section on the home page
7. Verify:
   - Images are visible (if products with images exist in DB)
   - OR placeholders show (if no products with images exist)
   - Network tab shows `/uploads/` requests going to Status 200 (found) or 404 (file not found on API)
   - Network tab shows requests are NOT failing due to wrong server

### 6. Check Console for Errors
In browser DevTools Console tab:
- Should NOT see CORS errors
- Should NOT see 404 errors for `/uploads/` from wrong origin
- May see legitimate 404s if images don't exist in DB (that's OK)
- Should see: "Fetching recently added products..." (if logging enabled)

## Expected Behavior

### With Product Images in Database
- "Recently added items" card shows up to 4 actual product images
- Images load successfully from `/uploads/{companyId}/{variantId}/image.jpg`
- Network tab shows 200 OK for image requests
- No console errors

### Without Product Images in Database
- "Recently added items" card shows 4 placeholders saying "Item 1", "Item 2", etc.
- No network requests for images (or requests that return 404 from API)
- No console errors
- This is expected behavior - the filtering logic removes products without images

## Troubleshooting

### Images Still Not Showing
1. Check if products exist with images:
   ```bash
   curl -k https://localhost:7182/api/Item/GetRecentlyAddedProducts?count=4
   # Look for products with imageUrls or thumbnailUrl in variants
   ```

2. Check if image files actually exist on disk:
   ```bash
   ls -la /home/runner/work/CanoEh/CanoEh/API/wwwroot/uploads/
   ```

3. Check browser console for specific error messages

4. Verify Vite proxy is working:
   ```bash
   # Should show proxy config
   cat /home/runner/work/CanoEh/CanoEh/Store/store.client/vite.config.ts | grep -A5 "uploads"
   ```

### Database Connection Errors
If you see "LocalDB is not supported on this platform":
1. The test database may not be set up
2. This is separate from the image display fix
3. You can test the proxy configuration without data using curl commands above

### CORS Errors
- CORS should not be an issue since proxy forwards requests to API
- If you see CORS errors, verify API is running and CORS policy includes https://localhost:64941

## Creating Test Data (Optional)
If you need products with images to test:

1. Upload product images via Seller app or API
2. OR create test products using API:
   ```bash
   curl -k -X POST https://localhost:7182/api/Item/CreateItem \
     -H "Content-Type: application/json" \
     -d '{
       "sellerID": "your-seller-id-guid",
       "name_en": "Test Product",
       "name_fr": "Produit de test",
       "categoryID": "category-id-guid",
       "variants": [{
         "price": 19.99,
         "stockQuantity": 10,
         "sku": "TEST-001",
         "imageUrls": "/uploads/test/image.jpg",
         "thumbnailUrl": "/uploads/test/thumb.jpg"
       }]
     }'
   ```

3. Note: You'll need valid GUIDs for sellerID and categoryID from your database

## Success Criteria
✅ Vite dev server starts successfully with updated config
✅ `/api` requests are proxied to API server (existing behavior)
✅ `/uploads` requests are proxied to API server (new behavior)
✅ No console errors related to image loading
✅ Images display OR placeholders show (depending on data availability)
✅ Network tab shows `/uploads` requests going to correct server
✅ All automated tests continue to pass

## Notes
- This fix only affects development environment (Vite dev server)
- In production, frontend and API typically share the same domain
- The proxy configuration makes development environment behave like production
- The actual image display logic in Home.tsx was already correct
- The fix is purely a development server configuration change
