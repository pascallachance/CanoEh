# CanoEh - E-commerce Application

CanoEh is a full-stack e-commerce web application built with .NET 8.0 backend and React/TypeScript frontend. It features user authentication, session management, email validation, and item/category management.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites
- .NET 8.0 SDK (confirmed working with 8.0.118)
- Node.js v20+ (detected v20.19.4)
- SQL Server LocalDB or SQLite for development

### Initial Setup and Build
Run these commands in order for first-time setup:

```bash
cd /path/to/CanoEh
dotnet restore  # Takes 3 seconds to 2 minutes (depends on cache). NEVER CANCEL - set timeout to 180+ seconds
dotnet build    # Takes ~20-60 seconds. NEVER CANCEL - set timeout to 120+ seconds
cd Store/store.client
npm install     # Takes <5 seconds typically
npm run build   # Takes ~4 seconds
npm run lint    # Quick lint check
```

### Running the Application

**Option 1: Store.Server (Full-stack SPA host - RECOMMENDED)**
```bash
cd Store/Store.Server
dotnet run      # Starts both backend API and frontend dev server
```
- Backend API: http://localhost:5199
- Frontend (SPA proxy): https://localhost:64941 (self-signed cert - click "Advanced" → "Proceed to localhost")
- Swagger API docs: http://localhost:5199/swagger

**Option 2: Standalone API Server**
```bash
cd API
dotnet run      # Standalone API only
```
- API: http://localhost:5269
- Swagger UI: http://localhost:5269/swagger (NOT AVAILABLE - standalone API has no Swagger UI)

**Option 3: Frontend Development**
```bash
cd Store/store.client
npm run dev     # Vite dev server only
```
- Frontend: https://localhost:64941

### Testing
```bash
cd /path/to/CanoEh
dotnet test     # Takes ~50 seconds. NEVER CANCEL - set timeout to 120+ seconds
```
**Expected Results:**
- Total: 202 tests
- Passed: 196 tests  
- Failed: 6 tests (KNOWN failing tests - do not fix unless specifically tasked)
- Tests in API.Tests project cover user authentication, password validation, session management

## Architecture Overview

### Backend (.NET 8.0)
- **API**: Standalone Web API server with Swagger documentation
- **Store.Server**: Combined API + SPA proxy server (primary development mode)
- **Domain**: Business logic and services
- **Infrastructure**: Data access, repositories, external services
- **Helpers**: Utility classes and common functionality
- **API.Tests**: xUnit test suite

### Frontend (React + TypeScript + Vite)
- **store.client**: React SPA with Vite build system
- Routes: `/login`, `/CreateUser`, `/RestorePassword`
- Features: User authentication, account creation, password recovery
- Build output: `dist/` directory

### Key Features
- JWT-based authentication with session management
- Email validation workflow
- User registration and login
- Item and category management
- CORS-enabled API for frontend communication

## Validation Workflows

### End-to-End Application Test
After making changes, ALWAYS validate the complete user flow:

1. **Start the application**:
   ```bash
   cd Store/Store.Server && dotnet run
   ```

2. **Access frontend**: Navigate to https://localhost:64941
   - Accept self-signed certificate warning (click "Advanced" → "Proceed to localhost")

3. **Test user registration flow**:
   - Click "Create account?" 
   - Fill out form with valid data (example: name="Test User", email="test@example.com", password="TestPass123!")
   - Verify form validation works (try invalid email, short password)
   - Submit and check for appropriate response

4. **Test login flow**:
   - Navigate to login page
   - Test with invalid credentials (should show error)
   - Test form validation (empty fields, invalid email)
   - Verify "Create account?" and "Forgot Password?" links work

5. **Verify API endpoints**:
   ```bash
   curl -s http://localhost:5199/weatherforecast  # Should return JSON weather data
   curl -s http://localhost:5199/swagger/v1/swagger.json | jq '.paths | keys[]'  # List API endpoints
   ```

6. **Test Swagger UI**: Navigate to http://localhost:5199/swagger
   - Verify Swagger UI loads and shows API documentation
   - Test at least one endpoint through the UI

### Build Validation
Before committing changes, ALWAYS run:
```bash
dotnet restore && dotnet build  # NEVER CANCEL - 180+ second timeout
cd Store/store.client && npm run lint && npm run build
dotnet test  # NEVER CANCEL - 120+ second timeout, expect 6 failing tests
```

## Development Configuration

### Database Configuration
- **API**: Uses SQL Server LocalDB (`(localdb)\\MSSQLLocalDB`) with `CanoEh` database
- **Store.Server**: Uses SQLite (`development.db` file)
- Connection strings in respective `appsettings.json` files

### JWT Configuration
- **API**: Secret from appsettings, issuer: `https://localhost:7182`, audience: `CanoEh`
- **Store.Server**: Development secret, issuer: `StoreApp`, audience: `StoreClient`

### CORS Configuration
- Frontend URL: `https://localhost:64941`
- Credentials allowed for authentication flows

### Email Settings
- Both projects configured for email validation feature
- Development mode: logs email content to debug output
- SMTP settings in appsettings.json (Brevo/Gmail configuration)

## Common Tasks and Locations

### Key File Locations
- **API Controllers**: `/API/Controllers/`
- **Domain Services**: `/Domain/Services/`
- **Repository Implementations**: `/Infrastructure/Repositories/Implementations/`
- **React Components**: `/Store/store.client/src/`
- **API Tests**: `/API.Tests/`

### Frequently Used Commands
```bash
# Quick build check
dotnet build --configuration Release

# Run specific test class
dotnet test --filter "ChangePasswordValidationShould"

# Frontend type checking
cd Store/store.client && npx tsc --noEmit

# View available API endpoints (Store.Server only)
curl -s http://localhost:5199/swagger/v1/swagger.json | jq '.paths | keys[]'

# View available API endpoints (standalone API server)  
curl -s http://localhost:5269/swagger/v1/swagger.json | jq '.paths | keys[]'
```

### Making Changes
- **API Changes**: Modify controllers in `/API/Controllers/`, update tests in `/API.Tests/`
- **Business Logic**: Update services in `/Domain/Services/`
- **Frontend Changes**: Modify React components in `/Store/store.client/src/`
- **Database Access**: Update repositories in `/Infrastructure/Repositories/`

## Troubleshooting

### Common Issues
- **Certificate Errors**: Accept self-signed certificate in browser for https://localhost:64941
- **API Not Accessible**: Ensure correct port (5269 for API, 5199 for Store.Server)
- **Frontend Build Errors**: Run `npm install` in `Store/store.client` directory
- **Test Failures**: 6 tests are expected to fail (existing issues)
- **Swagger UI 404**: Swagger UI only available on Store.Server (port 5199), NOT on standalone API (port 5269)

### Performance Notes
- **CRITICAL**: Build operations take 20 seconds to 2 minutes - use 120-180+ second timeouts
- **CRITICAL**: Test execution takes ~50 seconds - use 120+ second timeouts  
- **NEVER CANCEL** long-running builds or tests
- Frontend builds are fast (~4 seconds)
- Hot reload available in development mode

### Port Reference
- API standalone: `http://localhost:5269`
- Store.Server backend: `http://localhost:5199`
- Frontend dev server: `https://localhost:64941`
- API Swagger UI: `/swagger` endpoint on respective backend ports

## Screenshots
Working application shows:
- Clean "CanoEh!" branding
- User registration form with validation
- Login interface with "Create account?" and "Forgot Password?" links
- Professional UI styling with form validation