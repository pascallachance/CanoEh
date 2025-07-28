# Change Password Endpoint Documentation

## Overview
The Change Password endpoint allows authenticated users to change their password securely.

## Endpoint
`POST /api/User/ChangePassword`

## Authentication
Requires a valid JWT token in the Authorization header.

## Request Format
```json
{
  "username": "string",
  "currentPassword": "string", 
  "newPassword": "string",
  "confirmNewPassword": "string"
}
```

## Security Requirements
- User must be authenticated with a valid JWT token
- Users can only change their own password (verified via JWT claims)
- Current password must be provided and verified before allowing change
- New password must be at least 8 characters long
- New password must be different from current password
- New password and confirm password must match

## Validation Rules
- All fields are required
- Username must be at least 8 characters long
- Current password must be at least 8 characters long
- New password must be at least 8 characters long
- New password and confirm password must match
- New password must be different from current password

## Success Response (200 OK)
```json
{
  "username": "testuser123",
  "lastUpdatedAt": "2024-01-15T10:30:00Z",
  "message": "Password changed successfully."
}
```

## Error Responses

### 400 Bad Request
- Invalid request format
- Validation errors (password too short, passwords don't match, etc.)
- Current password is incorrect

### 403 Forbidden
- User trying to change another user's password

### 404 Not Found
- User not found

### 500 Internal Server Error
- Unexpected server error

## Example Usage

### Request
```http
POST /api/User/ChangePassword
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "username": "testuser123",
  "currentPassword": "oldpassword123",
  "newPassword": "newpassword456",
  "confirmNewPassword": "newpassword456"
}
```

### Success Response
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "username": "testuser123",
  "lastUpdatedAt": "2024-01-15T10:30:00Z",
  "message": "Password changed successfully."
}
```

### Error Response - Wrong Current Password
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

"Current password is incorrect."
```

### Error Response - Authorization Error
```http
HTTP/1.1 403 Forbidden
Content-Type: application/json

"You can only change your own password."
```

## Implementation Notes
- Passwords are hashed using Argon2 for security
- The system prevents password reuse (new password cannot be the same as current)
- All password validation follows the same rules as user registration
- The endpoint follows the existing authentication patterns in the application