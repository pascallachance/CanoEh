# User Restoration Database Schema Changes

This document describes the database schema changes required for the User Restoration feature.

## Overview

The User Restoration feature allows deleted users to restore their accounts via email verification. This requires adding two new fields to the existing `User` table.

## Database Changes Required

The following fields need to be added to the `User` table:

### New Columns

1. **restoreUserToken** (NVARCHAR(255), nullable)
   - Stores the restore token sent via email
   - Used to authenticate restoration requests
   - NULL when no restoration is in progress

2. **restoreUserTokenExpiry** (DATETIME2, nullable)  
   - Stores when the restore token expires (UTC)
   - Tokens expire 24 hours after generation
   - NULL when no restoration is in progress

## SQL Script to Add Columns

```sql
-- Add restore user token fields to User table
ALTER TABLE dbo.User 
ADD restoreUserToken NVARCHAR(255) NULL,
    restoreUserTokenExpiry DATETIME2 NULL;

-- Add index for performance on token lookups
CREATE INDEX IX_Users_RestoreUserToken 
ON dbo.User (restoreUserToken)
WHERE restoreUserToken IS NOT NULL;
```

## Column Descriptions

- **restoreUserToken**: Cryptographically secure random token (Base64 encoded, 32 bytes)
- **restoreUserTokenExpiry**: UTC timestamp when the token becomes invalid (24 hours from generation)

## Security Features

- **Token Expiration**: Restore tokens expire after 24 hours
- **Secure Token Generation**: Uses cryptographically secure random number generation
- **One-Time Use**: Tokens are cleared after successful restoration
- **Deleted User Only**: Tokens only work for users with `deleted = 1`

## Token Lifecycle

1. **Generation**: When `SendRestoreUserEmail` is called for a deleted user
2. **Storage**: Token and expiry are stored in the database
3. **Validation**: `RestoreUser` validates token exists and hasn't expired
4. **Cleanup**: Token is cleared when user is restored or token expires

## Example Token Flow

1. User requests restoration via email
2. System generates secure token: `"abc123def456..."`
3. Token stored with expiry: `DATEADD(HOUR, 24, GETUTCDATE())`
4. Email sent with restoration link containing token
5. User clicks link and submits token
6. System validates token is valid and not expired
7. User account is restored (`deleted = 0`) and token is cleared

## Repository Methods Added

The following methods were added to support these database changes:

- `FindDeletedByEmailAsync(string email)` - Find deleted users by email
- `FindByRestoreUserTokenAsync(string token)` - Find user by valid restore token
- `UpdateRestoreUserTokenAsync(string email, string token, DateTime expiry)` - Set restore token
- `RestoreUserByTokenAsync(string token)` - Restore user and clear token

## Relationship to Existing Features

This follows the same pattern as the existing password reset functionality:
- Similar to `passwordResetToken` and `passwordResetTokenExpiry` fields
- Same security principles and token lifecycle
- Consistent with existing email-based verification flows

## Index Considerations

The optional index on `restoreUserToken` improves performance for token lookups while avoiding overhead for NULL values (most users).

## Cleanup Strategy

Expired tokens can be cleaned up with a periodic job:

```sql
-- Clean up expired restore tokens (optional maintenance)
UPDATE dbo.User 
SET restoreUserToken = NULL, 
    restoreUserTokenExpiry = NULL
WHERE restoreUserTokenExpiry < GETUTCDATE();
```