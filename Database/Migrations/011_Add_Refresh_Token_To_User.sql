-- Migration: Add Refresh Token columns to User table
-- Date: 2026-02-23
-- Description: Adds refreshToken and refreshTokenExpiry columns to support JWT token refresh flow.
--              These columns are required by the /api/Login/refresh endpoint to validate and rotate
--              refresh tokens. Without these columns, token refresh always fails with 401 Unauthorized.

USE CanoEh;
GO

-- Add refreshToken column (nullable, stores the SHA-256 hash of the refresh token as a hex string)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[User]') AND name = 'refreshToken')
BEGIN
    ALTER TABLE dbo.[User]
    ADD refreshToken NVARCHAR(500) NULL;

    PRINT 'refreshToken column added to User table';
END
ELSE
BEGIN
    PRINT 'refreshToken column already exists in User table';
END
GO

-- Add refreshTokenExpiry column (nullable, stores the UTC expiry datetime of the refresh token)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[User]') AND name = 'refreshTokenExpiry')
BEGIN
    ALTER TABLE dbo.[User]
    ADD refreshTokenExpiry DATETIME2 NULL;

    PRINT 'refreshTokenExpiry column added to User table';
END
ELSE
BEGIN
    PRINT 'refreshTokenExpiry column already exists in User table';
END
GO

-- Add index on refreshToken for efficient lookups (only for non-NULL values)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.[User]') AND name = 'IX_User_RefreshToken')
BEGIN
    CREATE INDEX IX_User_RefreshToken ON dbo.[User](refreshToken) WHERE refreshToken IS NOT NULL;

    PRINT 'IX_User_RefreshToken index created on User table';
END
ELSE
BEGIN
    PRINT 'IX_User_RefreshToken index already exists on User table';
END
GO

-- Summary
PRINT 'Migration 011_Add_Refresh_Token_To_User completed successfully.';
PRINT '  - Ensured refreshToken column exists on dbo.[User].';
PRINT '  - Ensured refreshTokenExpiry column exists on dbo.[User].';
PRINT '  - Ensured IX_User_RefreshToken filtered index exists on dbo.[User](refreshToken).';
