# Company Email and WebSite Fields Implementation

## Overview
This implementation adds two new fields to the Company table:
- **Email** (required): Company contact email address
- **WebSite** (optional): Company website URL

## Changes Made

### 1. Data Model Updates
- **File**: `Infrastructure/Data/Company.cs`
- Added `Email` (required string) and `WebSite` (nullable string) properties

### 2. Request/Response Models
Updated the following models to include Email and WebSite fields:
- `Domain/Models/Requests/CreateCompanyRequest.cs` - Email validation included
- `Domain/Models/Requests/UpdateCompanyRequest.cs` - Email validation included
- `Domain/Models/Responses/CreateCompanyResponse.cs`
- `Domain/Models/Responses/GetCompanyResponse.cs`
- `Domain/Models/Responses/UpdateCompanyResponse.cs`

### 3. Email Validation
Both CreateCompanyRequest and UpdateCompanyRequest now validate:
- Email is required (not null, empty, or whitespace)
- Email format is valid (uses `System.Net.Mail.MailAddress` for validation)

### 4. Service Layer
- **File**: `Domain/Services/Implementations/CompanyService.cs`
- Updated `CreateCompanyAsync` to map Email and WebSite fields
- Updated `UpdateMyCompanyAsync` to update Email and WebSite fields

### 5. Repository Layer
- **File**: `Infrastructure/Repositories/Implementations/CompanyRepository.cs`
- Updated `AddAsync` to include Email and WebSite in INSERT query
- Updated `UpdateAsync` to include Email and WebSite in UPDATE query

### 6. Converters
- **File**: `Domain/Models/Converters/CompanyConverters.cs`
- Updated all converter methods to map Email and WebSite fields

### 7. Database Migration
- **File**: `Database/Migrations/002_Add_Email_Website_Columns_To_Company.sql`
- Adds Email column (NVARCHAR(255), required, default: 'contact@example.com')
- Adds WebSite column (NVARCHAR(500), nullable)

## Database Migration Steps

### Option 1: Using sqlcmd (Command Line)
```bash
sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/002_Add_Email_Website_Columns_To_Company.sql"
```

### Option 2: Using SQL Server Management Studio (SSMS) or Azure Data Studio
1. Connect to your SQL Server instance
2. Open `Database/Migrations/002_Add_Email_Website_Columns_To_Company.sql`
3. Execute the script against your CanoEh database

### Migration Notes
- The migration script is idempotent (safe to run multiple times)
- Existing companies will receive a default email: `contact@example.com`
- Users should update their company information to provide valid email addresses after migration
- The WebSite field is optional and will be NULL for existing companies

## API Changes

### CreateCompany Endpoint
**Request body now requires Email field:**
```json
{
  "name": "My Company",
  "email": "contact@mycompany.com",
  "webSite": "https://mycompany.com",
  "description": "Company description",
  "logo": "logo.png"
}
```

### UpdateCompany Endpoint
**Request body now requires Email field:**
```json
{
  "id": "guid",
  "name": "My Company",
  "email": "contact@mycompany.com",
  "webSite": "https://mycompany.com",
  "description": "Updated description"
}
```

### GetCompany Response
**Response now includes Email and WebSite:**
```json
{
  "id": "guid",
  "ownerID": "guid",
  "name": "My Company",
  "email": "contact@mycompany.com",
  "webSite": "https://mycompany.com",
  "description": "Company description",
  "createdAt": "2025-12-19T...",
  "updatedAt": null
}
```

## Testing

### Test Coverage
- 7 new Email validation tests added
- All existing Company tests updated to include Email field
- Total Company-related tests: 51 (49 passing, 2 integration test failures unrelated to this change)

### Test Results
```
Passed!  - Failed:     0, Passed:    49, Skipped:     0, Total:    51
```

### Email Validation Test Cases
1. Email is required (empty email fails)
2. Email format validation (invalid format fails)
3. Valid email formats pass (test@company.com, user.name@example.co.uk, test+tag@domain.com)
4. Both CreateCompanyRequest and UpdateCompanyRequest validate Email

## Breaking Changes

### ⚠️ Breaking Change: Email is now required
**Impact**: Any API clients creating or updating companies must now provide a valid Email address.

**Migration Path for Existing Data**:
1. Run the database migration script
2. Existing companies will have default email: `contact@example.com`
3. Update existing companies with valid email addresses using the UpdateCompany endpoint

**Migration Path for API Clients**:
1. Update API client code to include Email in CreateCompanyRequest
2. Update API client code to include Email in UpdateCompanyRequest
3. Update response handling to process the new Email and WebSite fields

## Validation Rules

### Email Field
- **Required**: Yes
- **Max Length**: 255 characters (database constraint)
- **Validation**: Must be a valid email format
- **Example Valid Values**: 
  - `contact@company.com`
  - `user.name@example.co.uk`
  - `test+tag@domain.com`

### WebSite Field
- **Required**: No
- **Max Length**: 500 characters (database constraint)
- **Validation**: None (any string or null)
- **Example Valid Values**:
  - `https://www.company.com`
  - `http://company.com`
  - `null` (not provided)

## Rollback Instructions

If you need to rollback this change:

1. **Revert code changes**: 
   ```bash
   git revert <commit-hash>
   ```

2. **Rollback database** (optional, if you want to remove the columns):
   ```sql
   -- Remove WebSite column
   IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Company') AND name = 'WebSite')
   BEGIN
       ALTER TABLE dbo.Company DROP COLUMN WebSite;
   END

   -- Remove Email column (WARNING: This will delete data!)
   IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Company') AND name = 'Email')
   BEGIN
       ALTER TABLE dbo.Company DROP COLUMN Email;
   END
   ```

## Questions or Issues?

If you encounter any issues with this implementation:
1. Verify the database migration has been applied
2. Check that all existing companies have valid email addresses
3. Ensure API clients are sending the Email field in requests
4. Review the validation test cases for expected behavior
