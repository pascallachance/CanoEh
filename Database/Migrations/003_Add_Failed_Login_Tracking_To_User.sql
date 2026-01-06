-- Migration: Add Failed Login Tracking columns to User table
-- Date: 2026-01-06
-- Description: Adds FailedLoginAttempts and LastFailedLoginAttempt columns to prevent brute force attacks

-- Add FailedLoginAttempts column with default value 0 for existing users
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[User]') AND name = 'failedLoginAttempts')
BEGIN
    ALTER TABLE dbo.[User]
    ADD failedLoginAttempts INT NOT NULL DEFAULT 0;
    
    PRINT 'FailedLoginAttempts column added to User table with default value 0';
END
ELSE
BEGIN
    PRINT 'FailedLoginAttempts column already exists in User table';
END
GO

-- Add LastFailedLoginAttempt column (nullable)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[User]') AND name = 'lastFailedLoginAttempt')
BEGIN
    ALTER TABLE dbo.[User]
    ADD lastFailedLoginAttempt DATETIME2 NULL;
    
    PRINT 'LastFailedLoginAttempt column added to User table';
END
ELSE
BEGIN
    PRINT 'LastFailedLoginAttempt column already exists in User table';
END
GO
