# Password Recovery API Documentation

This document describes the password recovery functionality added to the BaseApp API.

## Overview

The password recovery feature allows users to reset their password when they forget it. The process involves two steps:

1. **Request Password Reset**: User provides their email address
2. **Reset Password**: User uses the token received via email to set a new password

## API Endpoints

### 1. Request Password Reset

**Endpoint**: `POST /api/PasswordReset/ForgotPassword`

**Description**: Initiates the password reset process by sending a reset email to the user.

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
  "message": "If the email address exists in our system, you will receive a password reset link shortly."
}
```

**Security Note**: This endpoint always returns a success response to prevent email enumeration attacks, regardless of whether the email exists in the system.

### 2. Reset Password

**Endpoint**: `POST /api/PasswordReset/ResetPassword`

**Description**: Resets the user's password using a valid reset token.

**Request Body**:
```json
{
  "token": "reset-token-from-email",
  "newPassword": "newSecurePassword123",
  "confirmNewPassword": "newSecurePassword123"
}
```

**Success Response** (200 OK):
```json
{
  "message": "Password has been reset successfully.",
  "resetAt": "2025-07-30T15:30:00Z"
}
```

**Error Response** (400 Bad Request):
```json
{
  "error": "Invalid or expired reset token."
}
```

## Security Features

- **Token Expiration**: Reset tokens expire after 24 hours
- **Secure Token Generation**: Uses cryptographically secure random token generation
- **Email Enumeration Protection**: Always returns success for forgot password requests
- **Password Validation**: Enforces minimum password length (8 characters)
- **One-Time Use**: Reset tokens are cleared after successful password reset

## Email Integration

When a password reset is requested:
1. A secure token is generated and stored with 24-hour expiration
2. An email is sent to the user with a reset link
3. The reset link contains the token as a query parameter
4. Users click the link and provide their new password

## Database Changes

The following fields were added to the `Users` table:
- `passwordResetToken`: Stores the reset token (nullable)
- `passwordResetTokenExpiry`: Stores when the token expires (nullable)

## Example Usage Flow

1. **User forgets password and requests reset**:
   ```bash
   curl -X POST https://localhost:7182/api/PasswordReset/ForgotPassword \
     -H "Content-Type: application/json" \
     -d '{"email": "user@example.com"}'
   ```

2. **User receives email with reset link**:
   ```
   https://localhost:7182/api/PasswordReset/ResetPassword?token=abc123...
   ```

3. **User submits new password with token**:
   ```bash
   curl -X POST https://localhost:7182/api/PasswordReset/ResetPassword \
     -H "Content-Type: application/json" \
     -d '{
       "token": "abc123...",
       "newPassword": "newSecurePassword123",
       "confirmNewPassword": "newSecurePassword123"
     }'
   ```

## Error Handling

The API uses consistent error handling with appropriate HTTP status codes:
- `400 Bad Request`: Validation errors, invalid tokens
- `410 Gone`: User account is deleted/inactive
- `500 Internal Server Error`: Unexpected server errors

All errors return a descriptive error message to help with debugging.