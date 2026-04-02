-- =============================================
-- Migration: Narrow Item Name Columns to NVARCHAR(300)
-- Date: 2026-04-02
-- Description:
--   Alter Name_en and Name_fr on dbo.Item to NVARCHAR(300).
--   The previous limit was NVARCHAR(2000).  Any rows whose Name_en
--   or Name_fr already exceed 300 characters will cause this migration
--   to abort with an error so that no data is silently truncated.
--   Run the migration only after verifying (or cleaning up) such rows.
--
--   DATALENGTH(col)/2 is used instead of LEN() because LEN ignores
--   trailing spaces and would miss values whose visible length is
--   exactly 300 only because of trailing blanks.
--   THROW is used to abort execution deterministically; RAISERROR at
--   severity 16 does not reliably prevent subsequent statements from
--   running.
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
    IF EXISTS (SELECT 1 FROM dbo.Item WHERE DATALENGTH(Name_en) / 2 > 300)
    BEGIN
        THROW 50001, 'Cannot narrow dbo.Item.Name_en to NVARCHAR(300): one or more existing rows exceed 300 characters.', 1;
    END

    ALTER TABLE dbo.Item
        ALTER COLUMN Name_en NVARCHAR(300) NOT NULL;
    PRINT 'Column dbo.Item.Name_en narrowed to NVARCHAR(300).';
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
    IF EXISTS (SELECT 1 FROM dbo.Item WHERE DATALENGTH(Name_fr) / 2 > 300)
    BEGIN
        THROW 50002, 'Cannot narrow dbo.Item.Name_fr to NVARCHAR(300): one or more existing rows exceed 300 characters.', 1;
    END

    ALTER TABLE dbo.Item
        ALTER COLUMN Name_fr NVARCHAR(300) NOT NULL;
    PRINT 'Column dbo.Item.Name_fr narrowed to NVARCHAR(300).';
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
