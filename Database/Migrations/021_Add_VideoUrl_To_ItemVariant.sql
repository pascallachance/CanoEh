-- =============================================
-- Migration: Add VideoUrl Column To ItemVariant
-- Date: 2026-04-24
-- Description:
--   Adds VideoUrl column to the ItemVariant table to store
--   an optional product video URL for each variant.
-- =============================================

USE CanoEh;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ItemVariant') AND name = 'VideoUrl'
)
BEGIN
    ALTER TABLE dbo.ItemVariant
    ADD VideoUrl NVARCHAR(500) NULL;

    PRINT 'Added VideoUrl column to ItemVariant table.';
END
ELSE
BEGIN
    PRINT 'VideoUrl column already exists in ItemVariant table - skipping.';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 021 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Added VideoUrl (NVARCHAR(500) NULL) to ItemVariant';
PRINT '==============================================';
GO
