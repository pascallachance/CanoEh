# Image Display Fix for Recently Added Products

## Problem
Product images from GetRecentlyAddedProducts API endpoint were not displaying in the Store and Seller frontend applications. When the application attempted to load product images, the browser would receive 404 errors because the images were not found on the frontend dev server.

## Root Cause
The issue was caused by a missing proxy configuration in the Vite development server setup for both Store and Seller applications.

### How It Worked Before
1. The API's `LocalFileStorageService` saves uploaded product images to:
   - Physical location: `{API_ROOT}/wwwroot/uploads/{companyId}/{variantId}/{filename}.jpg`
   - Returns URL: `/uploads/{companyId}/{variantId}/{filename}.jpg`

2. The API serves these files using ASP.NET Core's static file middleware (`app.UseStaticFiles()` in Program.cs)

3. The frontend applications (Store and Seller) run on their own dev servers:
   - Store: `https://localhost:64941`
   - Seller: `https://localhost:62209`

4. The API server runs on:
   - `https://localhost:7182`

### The Problem
When the frontend received image URLs like `/uploads/{companyId}/{variantId}/image.jpg` from the API and tried to display them:
```html
<img src="/uploads/12345-67890/variant-id/image.jpg" />
```

The browser would try to load:
```
https://localhost:64941/uploads/12345-67890/variant-id/image.jpg  (Store)
https://localhost:62209/uploads/12345-67890/variant-id/image.jpg  (Seller)
```

But these files don't exist on the Vite dev servers! They only exist on the API server at:
```
https://localhost:7182/uploads/12345-67890/variant-id/image.jpg
```

This resulted in 404 Not Found errors and blank images.

### Why API Requests Worked
The Vite configurations already had a proxy for `/api` requests:
```typescript
proxy: {
    '^/api': {
        target: 'https://localhost:7182',
        secure: true,  // or false for seller
        changeOrigin: true
    }
}
```

This meant API calls like `/api/Item/GetRecentlyAddedProducts` were correctly forwarded to the API server. But image requests to `/uploads/...` were NOT proxied and failed.

## Solution
Added a proxy configuration for `/uploads` requests in both frontend applications' `vite.config.ts` files:

### Store Application (Store/store.client/vite.config.ts)
```typescript
server: {
    proxy: {
        '^/api': {
            target: 'https://localhost:7182',
            secure: true,
            changeOrigin: true
        },
        '^/uploads': {  // NEW: Proxy upload requests
            target: 'https://localhost:7182',
            secure: true,
            changeOrigin: true
        }
    },
    // ... rest of config
}
```

### Seller Application (Seller/seller.client/vite.config.ts)
```typescript
server: {
    proxy: {
        '^/api': {
            target: 'https://localhost:7182',
            secure: false, // Allow self-signed certificates in development
            changeOrigin: true
        },
        '^/uploads': {  // NEW: Proxy upload requests
            target: 'https://localhost:7182',
            secure: false, // Allow self-signed certificates in development
            changeOrigin: true
        }
    },
    // ... rest of config
}
```

### How It Works Now
1. Frontend receives product data with image URLs like `/uploads/{companyId}/{variantId}/image.jpg`
2. When browser tries to load the image, Vite dev server intercepts the request
3. Vite proxy forwards the request to `https://localhost:7182/uploads/...`
4. API server serves the file from its `wwwroot/uploads/` directory
5. Image displays successfully in the frontend

## Impact
- **Store Application**: Product images in "Recently added items" section now display correctly
- **Seller Application**: Product images display correctly wherever they are used
- No changes required to API code or database
- No changes required to frontend rendering logic
- Only configuration change needed

## Testing
### Manual Testing
1. Start the API: `cd API && dotnet run --launch-profile https`
2. Start the Store frontend: `cd Store/store.client && npm run dev`
3. Navigate to `https://localhost:64941`
4. Verify images in "Recently added items" card display correctly (assuming products with images exist in database)
5. Check browser Network tab - `/uploads/` requests should return 200 OK (or 404 if file doesn't exist, but NOT because of wrong server)

### Automated Testing
All existing tests continue to pass:
```bash
cd Store/store.client
npm test -- Home.ImageFiltering.test.tsx
# 6/6 tests pass
```

## Production Deployment
This fix is specific to the development environment where:
- Frontend runs on Vite dev server (localhost:64941 or localhost:62209)
- API runs on a different port (localhost:7182)

In production, if both frontend and API are served from the same domain/port (e.g., via reverse proxy or SPA hosting), the `/uploads/` URLs would naturally resolve to the correct location and this proxy would not be needed.

## Related Files
- `Store/store.client/vite.config.ts` - Store frontend Vite configuration
- `Seller/seller.client/vite.config.ts` - Seller frontend Vite configuration
- `API/Program.cs` - API static files configuration (line 291: `app.UseStaticFiles()`)
- `Infrastructure/Services/LocalFileStorageService.cs` - File upload service that generates `/uploads/` URLs
- `Store/store.client/src/components/Home.tsx` - Component that displays recently added products with images

## Alternative Solutions Considered
1. **Use absolute URLs in API responses**: Would require changing `LocalFileStorageService.GetFileUrl()` to return full URLs like `https://localhost:7182/uploads/...`. Rejected because:
   - Couples API responses to specific deployment URL
   - Breaks when API is accessed via different URLs (load balancer, reverse proxy, etc.)
   - Relative URLs are more flexible and standard

2. **Serve images from frontend build**: Would require copying uploaded images to frontend build output. Rejected because:
   - Images are dynamically uploaded via API
   - Would need complex synchronization between API storage and frontend builds
   - Doesn't work with separate API and frontend deployments

3. **Use cloud storage with absolute URLs**: Would work but is overkill for development. Could be considered for production.

The Vite proxy solution is the cleanest for development as it:
- Requires minimal configuration change
- Matches how production reverse proxies typically work
- Preserves relative URLs (best practice)
- No code changes required
