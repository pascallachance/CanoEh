# Multilingual Email Support Implementation

## Overview
This implementation adds support for sending emails in the user's preferred language (English or French) for the following email types:
- Email validation
- Password reset
- Account restoration

## Files Changed

### Core Changes
1. **Infrastructure/Data/User.cs**
   - Added `Language` property (string, defaults to "en")

2. **Helpers/Common/EmailContent.cs** (NEW)
   - Static helper class with methods for generating multilingual email content
   - `GetEmailValidation()` - Email validation content
   - `GetPasswordReset()` - Password reset content
   - `GetRestoreUser()` - Account restoration content
   - Each method returns a tuple of (Subject, Body) in the appropriate language

3. **Infrastructure/Services/EmailService.cs**
   - Modified `SendEmailValidationAsync()` to use `EmailContent.GetEmailValidation()`
   - Modified `SendPasswordResetAsync()` to use `EmailContent.GetPasswordReset()`
   - Modified `SendRestoreUserEmailAsync()` to use `EmailContent.GetRestoreUser()`

4. **Infrastructure/Repositories/Implementations/UserRepository.cs**
   - Updated `AddAsync()` to include Language field in INSERT query
   - Updated `UpdateAsync()` to include Language field in UPDATE query

5. **Domain/Models/Requests/CreateUserRequest.cs**
   - Added `Language` property (string, defaults to "en")

6. **Domain/Models/Requests/UpdateUserRequest.cs**
   - Added `Language` property (string?, optional for updates)

7. **Domain/Services/Implementations/UserService.cs**
   - Updated `CreateUserAsync()` to set Language when creating new user
   - Updated `UpdateUserAsync()` to update Language if provided

### Testing
8. **API.Tests/EmailContentShould.cs** (NEW)
   - 14 comprehensive tests covering:
     - English and French content generation
     - Null language handling (defaults to English)
     - Unknown language handling (defaults to English)
     - Case-insensitive language codes

### Database
9. **Database/Migrations/001_Add_Language_Column_To_User.sql** (NEW)
   - Idempotent SQL script to add Language column to User table
   - Sets default value of 'en' for existing users

10. **Database/Migrations/README.md** (NEW)
    - Documentation for applying database migrations

## Supported Languages
- **en** (English) - Default language
- **fr** (French) - Fully supported
- **Unknown codes** - Falls back to English

## Language Selection
- Users can specify language during registration via `CreateUserRequest.Language`
- Users can update language via `UpdateUserRequest.Language`
- Language is case-insensitive (en, EN, En all work)

## Email Content Structure
Each email type has:
- Localized subject line
- Localized greeting with user's name
- Localized body content
- Localized signature

## Database Migration
To apply the migration to your database:

```bash
# For SQL Server LocalDB
sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/001_Add_Language_Column_To_User.sql"

# For SQL Server
sqlcmd -S your-server -d CanoEh -U your-username -P your-password -i "Database/Migrations/001_Add_Language_Column_To_User.sql"
```

Or execute the script directly in SQL Server Management Studio or Azure Data Studio.

## Testing Results
- **14 new tests added** - All passing
- **332 total passing tests** (up from 318)
- **0 security vulnerabilities** introduced
- **0 regressions** in existing functionality

## Notes
- All changes are backward compatible
- Existing users will default to English ('en')
- The implementation follows the existing pattern used in `ForgotPasswordResponse`
- Email content is maintained in code (not in resource files) for simplicity
