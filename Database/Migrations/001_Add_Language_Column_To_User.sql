-- Migration: Add Language column to User table
-- Date: 2025-12-09
-- Description: Adds a Language column to support multilingual email notifications

-- Add Language column with default value 'en' for existing users
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[User]') AND name = 'language')
BEGIN
    ALTER TABLE dbo.[User]
    ADD language NVARCHAR(10) NOT NULL DEFAULT 'en';
    
    PRINT 'Language column added to User table with default value "en"';
END
ELSE
BEGIN
    PRINT 'Language column already exists in User table';
END
GO
