-- =============================================
-- Migration: Expand Item Description Columns to NVARCHAR(3000)
-- Date: 2026-04-08
-- Description:
--   Alter Description_en and Description_fr on dbo.Item
--   to NVARCHAR(3000) so that sellers can enter up to 3000 characters
--   in those fields without hitting a truncation error.
--
--   The condition uses max_length / 2 < 3000 (NVARCHAR stores 2 bytes per
--   character, so NVARCHAR(3000) has max_length = 6000; NVARCHAR(MAX) has
--   max_length = -1).  We only expand columns that are still narrower than
--   3000 characters.  Columns already at NVARCHAR(MAX) are left untouched.
-- =============================================

USE CanoEh;
GO

-- =============================================
-- dbo.Item.Description_en
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Description_en'
      AND max_length > 0          -- exclude NVARCHAR(MAX) which has max_length = -1
      AND max_length / 2 < 3000   -- only expand columns narrower than 3000 chars
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Description_en NVARCHAR(3000) NULL;
    PRINT 'Column dbo.Item.Description_en expanded to NVARCHAR(3000).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Description_en is already NVARCHAR(3000) or wider - skipping.';
END
GO

-- =============================================
-- dbo.Item.Description_fr
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Description_fr'
      AND max_length > 0
      AND max_length / 2 < 3000
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Description_fr NVARCHAR(3000) NULL;
    PRINT 'Column dbo.Item.Description_fr expanded to NVARCHAR(3000).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Description_fr is already NVARCHAR(3000) or wider - skipping.';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 018 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Expanded Item.Description_en  to NVARCHAR(3000) (if narrower than 3000)';
PRINT '  - Expanded Item.Description_fr  to NVARCHAR(3000) (if narrower than 3000)';
PRINT '  - Columns already at NVARCHAR(MAX) were left untouched.';
PRINT '==============================================';
GO
