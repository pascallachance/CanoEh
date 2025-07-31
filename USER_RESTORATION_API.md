# User Restoration API Documentation

This document describes the user restoration functionality added to the BaseApp API, which allows deleted users to restore their accounts.

## Overview

The user restoration feature allows deleted users to restore their accounts when they realize they want to return. The process involves two steps:

1. **Request Account Restoration**: User provides their email address to request restoration
2. **Restore Account**: User uses the token received via email to restore their account

## API Endpoints

### 1. Send Restore User Email

**Endpoint**: `POST /api/User/SendRestoreUserEmail`

**Description**: Initiates the account restoration process by sending a restoration email to deleted users.

**Request Body**:
```json
{
  "email": "user@example.com"
}
```

**Response** (Always returns 200 OK for security):
```json
{
  "email": "user@example.com",
  "message": "If the email address corresponds to a deleted account, you will receive a restore account link shortly."
}
```

**Security Note**: This endpoint always returns a success response to prevent email enumeration attacks, regardless of whether the email corresponds to a deleted user account.

### 2. Restore User

**Endpoint**: `POST /api/User/RestoreUser`

**Description**: Restores a deleted user account using a valid restoration token.

**Request Body**:
```json
{
  "token": "restore-token-from-email"
}
```

**Success Response** (200 OK):
```json
{
  "username": "restoreduser",
  "message": "Your account has been successfully restored."
}
```

**Error Response** (404 Not Found):
```json
{
  "error": "Invalid or expired restore token."
}
```

## Security Features

- **Token Expiration**: Restore tokens expire after 24 hours
- **Secure Token Generation**: Uses cryptographically secure random token generation
- **Email Enumeration Protection**: Always returns success for restore email requests
- **One-Time Use**: Restore tokens are cleared after successful account restoration
- **Deleted Users Only**: Restoration only works for users with `deleted = true`

## Email Integration

When account restoration is requested:
1. System checks if email corresponds to a deleted user account
2. If found, a secure token is generated and stored with 24-hour expiration
3. An email is sent to the user with a restoration link
4. The restoration link contains the token as a query parameter
5. Users click the link and submit the token to restore their account

## Database Integration

The restoration feature uses two new fields in the `Users` table:
- `restoreUserToken`: Stores the restoration token (nullable)
- `restoreUserTokenExpiry`: Stores when the token expires (nullable)

See `USER_RESTORATION_DATABASE_SCHEMA.md` for complete database setup instructions.

## Example Usage Flow

1. **User requests account restoration**:
   ```bash
   curl -X POST https://localhost:7182/api/User/SendRestoreUserEmail \
     -H "Content-Type: application/json" \
     -d '{"email": "deleted@example.com"}'
   ```

2. **User receives email with restoration link**:
   ```
   https://localhost:7182/api/User/RestoreUser?token=abc123...
   ```

3. **User submits restoration token**:
   ```bash
   curl -X POST https://localhost:7182/api/User/RestoreUser \
     -H "Content-Type: application/json" \
     -d '{"token": "abc123..."}'
   ```

## Error Handling

The API uses consistent error handling with appropriate HTTP status codes:
- `400 Bad Request`: Validation errors (empty email, empty token)
- `404 Not Found`: Invalid or expired restoration token
- `500 Internal Server Error`: Unexpected server errors

All errors return a descriptive error message to help with debugging.

## Email Template

The restoration email contains:
- Personalized greeting with username
- Clear explanation of the restoration request
- Direct link to restore the account
- Warning about link expiration (24 hours)
- Security notice if restoration wasn't requested

## Differences from Password Reset

While similar to password reset, user restoration has key differences:
- **Target Users**: Only works for deleted users (`deleted = 1`)
- **Action**: Restores account (`deleted = 0`) rather than changing password
- **Token Field**: Uses `restoreUserToken` instead of `passwordResetToken`
- **Repository Methods**: Uses specialized methods for deleted user lookup

## Integration with Existing Features

- **Email Service**: Reuses existing SMTP configuration and email infrastructure
- **Token Generation**: Uses same secure token generation as password reset
- **Validation Patterns**: Follows same request/response validation patterns
- **Error Handling**: Consistent with existing API error responses
- **Security Principles**: Same token-based authentication approach

## Testing

Comprehensive test coverage includes:
- Input validation scenarios
- Security edge cases
- Success and failure paths
- Repository interaction verification
- Email service integration

See `API.Tests/SendRestoreUserEmailShould.cs` and `API.Tests/RestoreUserShould.cs` for complete test implementation.

## Configuration Requirements

The feature requires the same email configuration as other email features:
- SMTP server settings
- Email credentials
- Base URL for generating restoration links

No additional configuration is needed beyond existing email setup.

## Monitoring and Logging

The feature includes debug logging for:
- Restoration email requests
- Token generation and validation
- Account restoration success/failure
- Email sending results

All operations are logged using the existing application logging framework.