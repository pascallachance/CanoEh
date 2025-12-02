# Session Management Database Schema

This file contains the SQL scripts required to create the Session table for the session management functionality.

## Session Table Creation Script

```sql
-- Create Session table
CREATE TABLE dbo.Session (
    SessionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LoggedOutAt DATETIME2 NULL,
    ExpiresAt DATETIME2 NOT NULL,
    UserAgent NVARCHAR(512) NULL,
    IpAddress NVARCHAR(45) NULL, -- Supports both IPv4 and IPv6
    
    -- Foreign key constraint
    CONSTRAINT FK_Sessions_Users FOREIGN KEY (UserId) 
        REFERENCES dbo.User(ID) ON DELETE CASCADE,
        
    -- Index for performance
    INDEX IX_Sessions_UserId (UserId),
    INDEX IX_Sessions_CreatedAt (CreatedAt),
    INDEX IX_Sessions_ExpiresAt (ExpiresAt),
    INDEX IX_Sessions_Active (UserId, LoggedOutAt, ExpiresAt) 
        WHERE LoggedOutAt IS NULL AND ExpiresAt > GETUTCDATE()
);
```

## Column Descriptions

- **SessionId**: Unique identifier for each session (Primary Key)
- **UserId**: Foreign key reference to the User table
- **CreatedAt**: Timestamp when the session was created (UTC)
- **LoggedOutAt**: Timestamp when the user logged out (NULL if still active)
- **ExpiresAt**: Timestamp when the session expires (UTC)
- **UserAgent**: Browser/client user agent string (optional)
- **IpAddress**: Client IP address (optional, supports IPv4/IPv6)

## Indexes

1. **IX_Sessions_UserId**: For finding sessions by user
2. **IX_Sessions_CreatedAt**: For sorting sessions by creation time
3. **IX_Sessions_ExpiresAt**: For cleanup of expired sessions
4. **IX_Sessions_Active**: Composite index for efficiently finding active sessions

## Session States

A session is considered **active** when:
- `LoggedOutAt` IS NULL (user hasn't logged out)
- `ExpiresAt` > GETUTCDATE() (session hasn't expired)

## Cleanup Query (Optional)

To clean up expired sessions (can be run as a scheduled job):

```sql
-- Delete expired sessions older than 30 days
DELETE FROM dbo.Session 
WHERE ExpiresAt < DATEADD(DAY, -30, GETUTCDATE());
```

## Usage Notes

1. The application automatically creates sessions on successful login
2. Sessions are marked as logged out (not deleted) when users logout
3. The `IsActive` property is computed in the application code
4. Session expiration is enforced by the application logic
5. No direct DELETE operations are performed on sessions by design