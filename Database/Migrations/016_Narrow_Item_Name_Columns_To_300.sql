-- =============================================
-- Migration: Narrow Item Name Columns to NVARCHAR(300)
-- Date: 2026-04-02
-- Description:
--   Alter Name_en and Name_fr on dbo.Item to NVARCHAR(300).
--   The previous limit was NVARCHAR(2000).  Any rows whose Name_en
--   or Name_fr already exceed 300 characters will cause this migration
--   to abort with an error so that no data is silently truncated.
--   Run the migration only after verifying (or cleaning up) such rows.
-- =============================================

USE CanoEh;
GO

-- =============================================
-- dbo.Item.Name_en
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Name_en'
      AND (max_length = -1 OR max_length / 2 > 300)
)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Item WHERE LEN(Name_en) > 300)
    BEGIN
        RAISERROR('Cannot narrow dbo.Item.Name_en to NVARCHAR(300): one or more existing rows exceed 300 characters.', 16, 1);
    END
    ELSE
    BEGIN
        ALTER TABLE dbo.Item
            ALTER COLUMN Name_en NVARCHAR(300) NOT NULL;
        PRINT 'Column dbo.Item.Name_en narrowed to NVARCHAR(300).';
    END
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Name_en is already NVARCHAR(300) or narrower - skipping.';
END
GO

-- =============================================
-- dbo.Item.Name_fr
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Name_fr'
      AND (max_length = -1 OR max_length / 2 > 300)
)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Item WHERE LEN(Name_fr) > 300)
    BEGIN
        RAISERROR('Cannot narrow dbo.Item.Name_fr to NVARCHAR(300): one or more existing rows exceed 300 characters.', 16, 1);
    END
    ELSE
    BEGIN
        ALTER TABLE dbo.Item
            ALTER COLUMN Name_fr NVARCHAR(300) NOT NULL;
        PRINT 'Column dbo.Item.Name_fr narrowed to NVARCHAR(300).';
    END
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Name_fr is already NVARCHAR(300) or narrower - skipping.';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 016 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Narrowed Item.Name_en to NVARCHAR(300) (if wider than 300)';
PRINT '  - Narrowed Item.Name_fr to NVARCHAR(300) (if wider than 300)';
PRINT '==============================================';
GO
