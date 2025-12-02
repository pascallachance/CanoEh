# Refresh Token Database Schema Changes

This document describes the database schema changes required for the Refresh Token feature.

## Overview

The Refresh Token feature enables persistent authentication by allowing users to stay logged in across browser sessions. This requires adding two new fields to the existing `Users` table to store refresh tokens securely.

## Database Changes Required

The following fields need to be added to the `Users` table:

### New Columns

1. **refreshToken** (NVARCHAR(255), nullable)
   - Stores the refresh token for the user
   - Used to generate new access tokens when they expire
   - NULL when user is logged out or no refresh token exists

2. **refreshTokenExpiry** (DATETIME2, nullable)  
   - Stores when the refresh token expires (UTC)
   - Tokens expire 30 days after generation by default
   - NULL when no refresh token exists

## SQL Script to Add Columns

```sql
-- Add refresh token fields to Users table
ALTER TABLE dbo.User 
ADD refreshToken NVARCHAR(255) NULL,
    refreshTokenExpiry DATETIME2 NULL;

-- Add index for performance on token lookups
CREATE INDEX IX_Users_RefreshToken 
ON dbo.User (refreshToken)
WHERE refreshToken IS NOT NULL;
```

## Column Descriptions

- **refreshToken**: Cryptographically secure random token (Base64 encoded, 64 bytes)
- **refreshTokenExpiry**: UTC timestamp when the token becomes invalid (30 days from generation)

## Security Features

- **HTTP-Only Cookies**: Refresh tokens are stored in HTTP-only cookies to prevent XSS attacks
- **Long-lived but Expiring**: Tokens expire after 30 days (configurable)
- **Secure Token Generation**: Uses cryptographically secure random number generation
- **Database Storage**: Tokens are stored in database for validation and revocation
- **Automatic Cleanup**: Tokens are cleared on logout

## Token Lifecycle

1. **Generation**: When user logs in successfully
2. **Storage**: Token and expiry are stored in the database and as HTTP-only cookie
3. **Usage**: Used to generate new access tokens when they expire (15 minutes)
4. **Refresh**: Each refresh generates a new refresh token (rotation)
5. **Cleanup**: Token is cleared when user logs out or token expires

## Authentication Flow

### Initial Login
1. User provides email/password
2. System validates credentials
3. Generate short-lived access token (15 minutes)
4. Generate long-lived refresh token (30 days)
5. Store refresh token in database with expiry
6. Set both tokens as HTTP-only cookies

### Token Refresh (Automatic)
1. Frontend detects 401 response from API
2. Automatically calls `/api/Login/refresh` endpoint
3. Backend validates refresh token from cookie
4. Generate new access token (15 minutes)
5. Generate new refresh token (30 days) - token rotation
6. Update database with new refresh token
7. Set new cookies for both tokens

### Logout
1. Clear refresh token from database
2. Delete both access and refresh token cookies

## Configuration

The refresh token expiry is configurable via `appsettings.json`:

```json
{
  "JwtSettings": {
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30
  }
}
```

## Repository Methods Added

The following methods were added to support these database changes:

- `FindByRefreshTokenAsync(string refreshToken)` - Find user by valid refresh token
- `UpdateRefreshTokenAsync(string email, string refreshToken, DateTime expiry)` - Set refresh token
- `ClearRefreshTokenAsync(string email)` - Clear refresh token on logout

## API Endpoints

### New Endpoints
- `POST /api/Login/refresh` - Refresh access token using refresh token

### Enhanced Endpoints
- `POST /api/Login/login` - Now issues both access and refresh tokens
- `POST /api/Login/logout` - Now clears refresh tokens

## Frontend Integration

The frontend includes an `ApiClient` class that:
- Automatically includes cookies in all requests
- Detects 401 responses and attempts token refresh
- Retries failed requests after successful token refresh
- Prevents multiple simultaneous refresh attempts

## Security Considerations

1. **Token Rotation**: Each refresh generates a new refresh token, invalidating the old one
2. **Database Validation**: All refresh tokens are validated against database storage
3. **Expiry Enforcement**: Expired tokens are rejected even if cookies exist
4. **HTTP-Only Cookies**: Tokens cannot be accessed via JavaScript
5. **Secure Transmission**: Cookies are marked secure in production
6. **CSRF Protection**: Uses SameSite cookie attribute

## Cleanup Strategy

Expired tokens can be cleaned up with a periodic job:

```sql
-- Clean up expired refresh tokens (optional maintenance)
UPDATE dbo.User 
SET refreshToken = NULL, 
    refreshTokenExpiry = NULL
WHERE refreshTokenExpiry < GETUTCDATE();
```

## Benefits

- **Persistent Authentication**: Users stay logged in across browser sessions
- **Improved Security**: Short-lived access tokens reduce attack window
- **Seamless UX**: Automatic token refresh is invisible to users
- **Scalable**: Database-backed tokens support multiple instances
- **Auditable**: Token usage can be tracked and monitored