-- =============================================
-- Migration: Expand Item Text Columns to NVARCHAR(2000)
-- Date: 2026-03-31
-- Description:
--   Alter Name_en, Name_fr, Description_en and Description_fr on dbo.Item
--   to NVARCHAR(2000) so that sellers can enter up to 2000 characters in
--   those fields without hitting a truncation error.
--
--   The condition uses max_length / 2 < 2000 (NVARCHAR stores 2 bytes per
--   character, so NVARCHAR(2000) has max_length = 4000; NVARCHAR(MAX) has
--   max_length = -1).  We only expand columns that are still narrower than
--   2000 characters.  Columns already at NVARCHAR(MAX) (from migration 014)
--   are left untouched since MAX already accommodates more than 2000 chars.
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
      AND max_length > 0          -- exclude NVARCHAR(MAX) which has max_length = -1
      AND max_length / 2 < 2000   -- only expand columns narrower than 2000 chars
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Name_en NVARCHAR(2000) NOT NULL;
    PRINT 'Column dbo.Item.Name_en expanded to NVARCHAR(2000).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Name_en is already NVARCHAR(2000) or wider - skipping.';
END
GO

-- =============================================
-- dbo.Item.Name_fr
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Name_fr'
      AND max_length > 0
      AND max_length / 2 < 2000
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Name_fr NVARCHAR(2000) NOT NULL;
    PRINT 'Column dbo.Item.Name_fr expanded to NVARCHAR(2000).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Name_fr is already NVARCHAR(2000) or wider - skipping.';
END
GO

-- =============================================
-- dbo.Item.Description_en
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Description_en'
      AND max_length > 0
      AND max_length / 2 < 2000
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Description_en NVARCHAR(2000) NULL;
    PRINT 'Column dbo.Item.Description_en expanded to NVARCHAR(2000).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Description_en is already NVARCHAR(2000) or wider - skipping.';
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
      AND max_length / 2 < 2000
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Description_fr NVARCHAR(2000) NULL;
    PRINT 'Column dbo.Item.Description_fr expanded to NVARCHAR(2000).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Description_fr is already NVARCHAR(2000) or wider - skipping.';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 015 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Expanded Item.Name_en         to NVARCHAR(2000) (if narrower than 2000)';
PRINT '  - Expanded Item.Name_fr         to NVARCHAR(2000) (if narrower than 2000)';
PRINT '  - Expanded Item.Description_en  to NVARCHAR(2000) (if narrower than 2000)';
PRINT '  - Expanded Item.Description_fr  to NVARCHAR(2000) (if narrower than 2000)';
PRINT '  - Columns already at NVARCHAR(MAX) were left untouched.';
PRINT '==============================================';
GO
