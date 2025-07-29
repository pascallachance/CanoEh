# Session Management API Documentation

This document describes the session management functionality added to the BaseApp API.

## Overview

The session management system automatically creates sessions during login and tracks user activity. Sessions provide an additional layer of security and auditing beyond JWT tokens.

## API Endpoints

### Login (Enhanced)

**POST** `/api/Login/login`

Enhanced to automatically create a session when login is successful.

#### Request Body
```json
{
  "username": "string",
  "password": "string",
  "userAgent": "string (optional)",
  "ipAddress": "string (optional)"
}
```

Note: `userAgent` and `ipAddress` are automatically captured from HTTP headers if not provided.

#### Response (Success)
```json
{
  "token": "jwt-token-string",
  "sessionId": "guid-string"
}
```

#### Response Codes
- **200 OK**: Login successful, session created
- **400 Bad Request**: Invalid input
- **401 Unauthorized**: Invalid credentials
- **403 Forbidden**: Email not validated
- **500 Internal Server Error**: Session creation failed

---

### Logout (Enhanced)

**POST** `/api/Login/logout`

Enhanced to mark sessions as logged out when sessionId is provided.

#### Headers
- **Authorization**: Bearer {jwt-token} (Required)
- **X-Session-Id**: {session-guid} (Optional)

#### Query Parameters
- **sessionId**: GUID (Optional) - Alternative to X-Session-Id header

#### Request Examples
```bash
# Logout with session ID in header
POST /api/Login/logout
Authorization: Bearer {jwt-token}
X-Session-Id: {session-guid}

# Logout with session ID in query parameter
POST /api/Login/logout?sessionId={session-guid}
Authorization: Bearer {jwt-token}

# Logout without session ID (traditional logout only)
POST /api/Login/logout
Authorization: Bearer {jwt-token}
```

#### Response (Success)
```json
{
  "message": "Logged out successfully.",
  "username": "string",
  "sessionId": "guid-string (if provided)"
}
```

#### Response Codes
- **200 OK**: Logout successful
- **401 Unauthorized**: Invalid or missing JWT token
- **404 Not Found**: Session not found (if sessionId provided)
- **500 Internal Server Error**: Logout operation failed

---

## Session Entity Properties

Sessions contain the following information:

```json
{
  "sessionId": "guid-string",
  "userId": "guid-string", 
  "createdAt": "datetime",
  "loggedOutAt": "datetime (nullable)",
  "expiresAt": "datetime",
  "userAgent": "string (nullable)",
  "ipAddress": "string (nullable)",
  "isActive": "boolean (computed)"
}
```

### Property Descriptions

- **sessionId**: Unique identifier for the session
- **userId**: Reference to the user who owns the session  
- **createdAt**: When the session was created (UTC)
- **loggedOutAt**: When the user logged out (null if still active)
- **expiresAt**: When the session expires (default: 24 hours from creation)
- **userAgent**: Browser/client information
- **ipAddress**: Client IP address
- **isActive**: Computed property: `loggedOutAt == null && expiresAt > now`

## Session Lifecycle

1. **Creation**: Automatic on successful login
2. **Active**: Session is usable (not logged out, not expired)
3. **Logged Out**: User explicitly logged out (loggedOutAt set)
4. **Expired**: Session passed its expiration time
5. **Inactive**: Either logged out or expired

## Business Rules

1. **No Session Deletion**: Sessions are never deleted, only marked as logged out
2. **Automatic Expiration**: Sessions expire 24 hours after creation by default
3. **Optional Session Tracking**: Logout can work with or without session tracking
4. **Client Information**: User agent and IP address are captured when available
5. **Multiple Sessions**: Users can have multiple active sessions

## Security Considerations

1. **Session IDs are UUIDs**: Cryptographically random, not sequential
2. **Logout is Optional**: Traditional JWT-only logout still works
3. **Session Validation**: Sessions can be validated independently of JWT tokens
4. **Audit Trail**: All sessions are preserved for auditing
5. **IP/Agent Tracking**: Helps detect suspicious activity

## Implementation Notes

- Sessions complement JWT tokens, they don't replace them
- Existing authentication flows remain unchanged
- Session creation failure doesn't prevent successful login
- Session logout failure doesn't prevent successful logout
- All session operations are optional and backward-compatible

## Database Schema

See `SESSION_DATABASE_SCHEMA.md` for the required database table structure.

## Error Handling

All session operations use the Result pattern and provide detailed error information:

- Session creation errors are logged but don't fail login
- Session logout errors are logged but don't fail logout  
- Invalid session IDs return 404 Not Found
- Database errors return 500 Internal Server Error