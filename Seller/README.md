# CanoEh Seller Application

The Seller application is a React frontend for sellers to manage their products, categories, and listings.

## Prerequisites

- .NET 8.0 SDK
- Node.js v20+
- The CanoEh API server running

## Quick Start

### 1. Start the API Server (Required)

The Seller client depends on the API server running on `https://localhost:7182`. 

```bash
# From the root directory
cd API
dotnet run --launch-profile https
```

The API server will start on:
- HTTPS: `https://localhost:7182`
- HTTP: `http://localhost:5269` 
- Swagger UI: `https://localhost:7182/swagger`

### 2. Start the Seller Client

```bash
# From the seller.client directory
cd Seller/seller.client
npm install
npm run dev
```

The Seller client will start on:
- HTTPS: `https://localhost:62209`

## Configuration

### API Proxy
The Vite development server is configured to proxy API requests:
- Client requests to `/api/*` are forwarded to `https://localhost:7182`
- See `vite.config.ts` for proxy configuration

### Environment Variables
- `VITE_API_SELLER_BASE_URL=https://localhost:7182` - Direct API base URL for authenticated requests

## Common Issues

### 500 Error on Category/Product API Calls

**Symptom**: Console error like:
```
GET https://localhost:62209/api/Category/GetAllCategories net::ERR_ABORTED 500 (Internal Server Error)
```

**Cause**: The API server is not running on `https://localhost:7182`

**Solution**: 
1. Start the API server with: `cd API && dotnet run --launch-profile https`
2. Verify it's running by visiting: `https://localhost:7182/swagger`
3. Refresh the Seller client

### CORS Errors for Direct API Calls

**Symptom**: Console errors about CORS policy blocking requests to `https://localhost:7182`

**Cause**: Some components (like session management) make direct API calls bypassing the proxy

**Solution**: This is expected behavior. The API server includes CORS configuration for the Seller client origin.

### Certificate Warnings

Both the API server and Seller client use self-signed certificates in development.
- Click "Advanced" → "Proceed to localhost (unsafe)" when prompted
- This is normal for local development

## Development

### File Structure
```
Seller/
├── seller.client/           # React TypeScript application
│   ├── src/
│   │   ├── components/      # React components
│   │   │   └── Seller/      # Seller-specific components
│   │   │       └── ProductsSection.tsx  # Product/category management
│   │   ├── utils/           # Utilities
│   │   │   └── apiClient.ts # API client with auth
│   │   └── App.tsx          # Main application
│   ├── vite.config.ts       # Vite configuration
│   └── package.json
└── README.md               # This file
```

### Making API Changes
When adding new API endpoints:
1. Add the endpoint to the appropriate controller in `/API/Controllers/`
2. The Seller client can access it via `/api/[Controller]/[Action]`
3. Use either relative URLs (for proxy) or the ApiClient class (for direct calls)

## Troubleshooting

1. **Build the solution first**: `dotnet build` from the root directory
2. **Check API health**: Visit `https://localhost:7182/swagger`
3. **Check client proxy**: Test `https://localhost:62209/api/Category/GetAllCategories` directly
4. **Check browser console**: Look for network errors and CORS issues