-- Migration: Add Email and WebSite columns to Company table
-- Date: 2025-12-19
-- Description: Adds Email (required) and WebSite (optional) columns to support company contact information

-- Add Email column with a temporary default value for existing records
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Company') AND name = 'Email')
BEGIN
    ALTER TABLE dbo.Company
    ADD Email NVARCHAR(255) NOT NULL DEFAULT 'contact@example.com';
    
    PRINT 'Email column added to Company table with default value "contact@example.com"';
END
ELSE
BEGIN
    PRINT 'Email column already exists in Company table';
END
GO

-- Add WebSite column (optional)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Company') AND name = 'WebSite')
BEGIN
    ALTER TABLE dbo.Company
    ADD WebSite NVARCHAR(500) NULL;
    
    PRINT 'WebSite column added to Company table';
END
ELSE
BEGIN
    PRINT 'WebSite column already exists in Company table';
END
GO

-- Note: After running this migration, existing companies will have a default email address.
-- Users should update their company information to provide a valid email address.
