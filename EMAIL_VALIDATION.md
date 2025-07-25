# Email Validation Feature

This document describes the email validation feature that has been implemented for the BaseApp API.

## Overview

All users must validate their email address before they can access authenticated API endpoints. When a user registers, they receive a validation email and their `ValidEmail` field is set to `false`. They cannot login until they click the validation link to set `ValidEmail` to `true`.

## Implementation Details

### Database Changes
- Added `ValidEmail` boolean field to the `User` entity
- Default value is `false` for new users
- Updated all response DTOs to include the `ValidEmail` field

### Email Service
- Created `IEmailService` interface and `EmailService` implementation
- Currently logs validation emails to debug output (can be extended for actual email sending)
- Generates validation links like: `https://localhost:7182/api/User/ValidateEmail/{userId}`

### Login Service Enhancement
- **NEW**: Added `SendValidationEmailAsync` method to `LoginService`
- Created `ILoginService` interface for better dependency injection
- LoginService now accepts `IEmailService` dependency

### API Changes

#### User Registration Flow
1. User calls `POST /api/User/CreateUser`
2. User is created with `ValidEmail = false`
3. Validation email is sent immediately
4. User receives email with validation link

#### Login Flow
1. User calls `POST /api/Login/login`
2. System checks username/password
3. **NEW**: System checks if `ValidEmail` is `true`
4. If email not validated, returns 403 Forbidden with message: "Please validate your email address before logging in"
5. If email is validated, returns JWT token

#### Manual Email Validation Request
1. **NEW**: User can request validation email resend via `LoginService.SendValidationEmailAsync`
2. System finds user by username
3. Checks if email needs validation
4. Sends validation email with link to ValidateEmail endpoint

#### Email Validation Flow
1. User clicks validation link from email
2. Calls `GET /api/User/ValidateEmail/{userId}`
3. System sets `ValidEmail = true` for the user
4. User can now login successfully

## API Endpoints

### Create User
```
POST /api/User/CreateUser
```
**Changed**: Now sends validation email and sets `ValidEmail = false`

### Validate Email
```
GET /api/User/ValidateEmail/{userId}
```
**NEW**: Validates the user's email address
- **Parameters**: `userId` (Guid) - The user ID from the validation link
- **Returns**: Success message if validation succeeds
- **Errors**: 404 if user not found, 400 if email already validated

### Login
```
POST /api/Login/login
```
**Changed**: Now checks email validation before allowing login
- **New Error**: 403 Forbidden if email not validated

### Send Validation Email (LoginService)
```csharp
await loginService.SendValidationEmailAsync("username");
```
**NEW**: Sends validation email to user
- **Parameters**: `username` (string) - The username to send validation email to
- **Returns**: `Result<bool>` indicating success or failure
- **Errors**: 
  - 400 if username is empty or user already validated
  - 404 if user not found
  - 500 if email sending fails

## Testing

The feature includes comprehensive tests:
- Unit tests for email validation endpoint
- Integration tests for login blocking when email not validated
- Tests for user creation with email sending
- Tests for email validation flow
- **NEW**: 7 unit tests for SendValidationEmailAsync functionality

All 43 tests pass, including the new email validation functionality.

## Configuration

The email service is registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
```

## Usage Example

```csharp
// Inject dependencies
var userRepository = new UserRepository(connectionString);
var emailService = new EmailService(emailOptions, logger);
var loginService = new LoginService(userRepository, emailService);

// Send validation email
var result = await loginService.SendValidationEmailAsync("username");
if (result.IsSuccess)
{
    Console.WriteLine("Validation email sent successfully");
}
else
{
    Console.WriteLine($"Failed to send email: {result.Error}");
}
```

## Future Enhancements

The current `EmailService` is a stub implementation. To enable actual email sending:

1. Add email configuration to `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@yourapp.com"
  }
}
```

2. Implement actual email sending in `EmailService.cs` using libraries like `MailKit` or `System.Net.Mail`

3. Add email templates for validation emails

## Error Messages

- **Login without email validation**: "Please validate your email address before logging in" (403 Forbidden)
- **User not found for validation**: "User not found." (404 Not Found)
- **Email already validated**: "Email is already validated." (400 Bad Request)
- **SendValidationEmail - Username required**: "Username is required." (400 Bad Request)
- **SendValidationEmail - User deleted**: "User account is deleted." (400 Bad Request)
- **SendValidationEmail - Email service failure**: "Failed to send validation email." (500 Internal Server Error)