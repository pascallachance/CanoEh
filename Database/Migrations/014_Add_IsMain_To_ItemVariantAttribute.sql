-- =============================================
-- Migration: Add IsMain Column to ItemVariantAttribute
-- Date: 2026-04-07
-- Description:
--   Adds an IsMain BIT column (default 0) to dbo.ItemVariantAttribute.
--   This column identifies the main variant attribute for a product,
--   enabling the seller UI to mark one attribute as the primary
--   variant selector (e.g., "Size" as main, not "Color").
--   After adding the column, backfills existing rows so that for each
--   ItemVariantID that has no IsMain=1 attribute, the first attribute
--   (by insertion order / Id) is set to IsMain=1.
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

-- Backfill: for each ItemVariantID that has no IsMain=1 attribute,
-- set the first attribute (lowest Id) as the main attribute.
WITH FirstAttribute AS (
    SELECT Id
    FROM dbo.ItemVariantAttribute a
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ItemVariantAttribute b
        WHERE b.ItemVariantID = a.ItemVariantID AND b.IsMain = 1
    )
      AND a.Id = (
        SELECT TOP 1 c.Id
        FROM dbo.ItemVariantAttribute c
        WHERE c.ItemVariantID = a.ItemVariantID
        ORDER BY c.Id
      )
)
UPDATE dbo.ItemVariantAttribute
SET IsMain = 1
WHERE Id IN (SELECT Id FROM FirstAttribute);

PRINT 'Backfill of IsMain completed: first attribute per ItemVariantID set to IsMain=1 where none was set.';
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 014 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Added IsMain column (BIT NOT NULL DEFAULT 0) to dbo.ItemVariantAttribute';
PRINT '  - Backfilled IsMain=1 for first attribute per ItemVariantID where none was set';
PRINT '==============================================';
GO
