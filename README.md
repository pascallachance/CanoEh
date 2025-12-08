# CanoEh! 🍁

CanoEh is a full-stack e-commerce web application built with .NET 8.0 backend and React/TypeScript frontend.

## Prerequisites

- .NET 8.0 SDK or later
- Node.js v20 or later
- SQL Server LocalDB (for API project)

## Quick Start

To run the application, you need to start both the backend API and the frontend development server.

### Option 1: Start Both Servers (Recommended)

Open two terminal windows:

**Terminal 1 - API Backend:**
```bash
cd API
dotnet run --launch-profile https
```
The API will be available at:
- HTTPS: https://localhost:7182
- HTTP: http://localhost:5269
- Swagger UI: https://localhost:7182/swagger

**Terminal 2 - Frontend Dev Server:**
```bash
cd Store/store.client
npm install  # Only needed first time
npm run dev
```
The frontend will be available at: https://localhost:64941

### Option 2: Using npm Scripts

```bash
# Terminal 1 - API
cd API && dotnet run --launch-profile https

# Terminal 2 - Frontend
cd Store/store.client && npm run dev
```

## First-Time Setup

1. **Restore .NET Dependencies:**
   ```bash
   dotnet restore
   ```

2. **Build the Solution:**
   ```bash
   dotnet build
   ```

3. **Install Frontend Dependencies:**
   ```bash
   cd Store/store.client
   npm install
   ```

4. **Trust HTTPS Certificates:**
   ```bash
   dotnet dev-certs https --trust
   ```

## Accessing the Application

Once both servers are running:

1. **Frontend Application:** https://localhost:64941
   - Login page: https://localhost:64941/login
   - Create account: https://localhost:64941/CreateUser
   - Restore password: https://localhost:64941/RestorePassword

2. **API Documentation:** https://localhost:7182/swagger

3. **Accept Certificate Warning:**
   - When first accessing https://localhost:64941, your browser will show a certificate warning
   - Click "Advanced" and then "Proceed to localhost" (or similar option in your browser)
   - This is normal for development with self-signed certificates

## Project Structure

```
CanoEh/
├── API/                    # ASP.NET Core Web API
├── Domain/                 # Business logic and services
├── Infrastructure/         # Data access and repositories
├── Helpers/               # Utility classes
├── Store/
│   └── store.client/      # React frontend (Vite)
├── Seller/
│   └── seller.client/     # Seller React frontend
├── shared/
│   └── canoeh-shared-ui/  # Shared UI components
└── API.Tests/             # Test projects
```

## Configuration

### API Configuration
- Connection String: SQL Server LocalDB
- JWT Settings: Configured in `API/appsettings.json`
- CORS: Allows frontend origins (ports 64941 and 62209)

### Frontend Configuration
- API Base URL: Set in `Store/store.client/.env`
- Default: `VITE_API_STORE_BASE_URL=https://localhost:7182`

## Testing

Run all tests:
```bash
dotnet test
```

Run tests for a specific project:
```bash
dotnet test API.Tests
```

## Development

### Frontend Development
```bash
cd Store/store.client
npm run dev      # Start dev server
npm run build    # Build for production
npm run lint     # Run ESLint
npm run preview  # Preview production build
```

### API Development
```bash
cd API
dotnet run --launch-profile https  # Run with HTTPS
dotnet watch run                    # Run with hot reload
```

## Troubleshooting

### ERR_CONNECTION_REFUSED
**Problem:** Browser shows "ERR_CONNECTION_REFUSED" when accessing https://localhost:64941

**Solution:** Make sure both servers are running:
1. Check if API is running: `curl -k https://localhost:7182/swagger/index.html`
2. Check if frontend is running: `curl -k https://localhost:64941/`
3. If not running, start them using the commands in Quick Start section

### Certificate Errors
**Problem:** Browser rejects HTTPS certificate

**Solution:**
1. Trust the development certificate: `dotnet dev-certs https --trust`
2. Accept the certificate warning in your browser (click "Advanced" → "Proceed to localhost")

### Port Already in Use
**Problem:** Error that port 7182 or 64941 is already in use

**Solution:**
1. Find process using the port: `lsof -i :7182` or `lsof -i :64941` (on macOS/Linux)
2. Kill the process or use different ports in configuration

## Environment Variables

### API (API/Properties/launchSettings.json)
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `EmailSettings__Username`: SMTP username
- `EmailSettings__Password`: SMTP password

### Frontend (Store/store.client/.env)
- `VITE_API_STORE_BASE_URL`: API base URL (default: https://localhost:7182)

## License

[Add license information here]

## Contributing

[Add contribution guidelines here]
