-- =============================================
-- Migration: Add ItemID Column to ItemVariantFeatures
-- Date: 2026-02-20
-- Description:
--   Add ItemID column to ItemVariantFeatures table to allow
--   item-level feature queries without joining through ItemVariant.
--   The column is nullable to preserve existing rows.
-- =============================================

USE CanoEh;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ItemVariantFeatures') AND name = 'ItemID')
BEGIN
    ALTER TABLE dbo.ItemVariantFeatures ADD ItemID UNIQUEIDENTIFIER NULL;
    PRINT 'Added column ItemID to ItemVariantFeatures';

    -- Back-fill ItemID from the related ItemVariant row
    UPDATE ivf
    SET ivf.ItemID = iv.ItemId
    FROM dbo.ItemVariantFeatures ivf
    INNER JOIN dbo.ItemVariant iv ON iv.Id = ivf.ItemVariantID;
    PRINT 'Back-filled ItemID values from ItemVariant';

    -- Add index for efficient item-level queries
    CREATE INDEX IX_ItemVariantFeatures_ItemID ON dbo.ItemVariantFeatures(ItemID);
    PRINT 'Created index IX_ItemVariantFeatures_ItemID';
END
ELSE
BEGIN
    PRINT 'Column ItemID already exists in ItemVariantFeatures - skipping';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 010 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Added ItemID column to ItemVariantFeatures (nullable)';
PRINT '  - Back-filled ItemID from ItemVariant table';
PRINT '  - Created index IX_ItemVariantFeatures_ItemID';
PRINT '==============================================';
GO
