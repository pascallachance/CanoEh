-- =============================================
-- Migration: Add IsMain Column to ItemVariantAttribute
-- Date: 2026-04-07
-- Description:
--   Adds an IsMain BIT column (default 0) to dbo.ItemVariantAttribute.
--   This column identifies the main variant attribute for a product,
--   enabling the seller UI to mark one attribute as the primary
--   variant selector (e.g., "Size" as main, not "Color").
-- =============================================

USE CanoEh;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ItemVariantAttribute')
      AND name = 'IsMain'
)
BEGIN
    ALTER TABLE dbo.ItemVariantAttribute
        ADD IsMain BIT NOT NULL DEFAULT 0;

    PRINT 'Column IsMain added to ItemVariantAttribute.';
END
ELSE
BEGIN
    PRINT 'Column IsMain already exists in ItemVariantAttribute - skipping.';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 014 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Added IsMain column (BIT NOT NULL DEFAULT 0) to dbo.ItemVariantAttribute';
PRINT '==============================================';
GO
