-- =============================================
-- Migration: Add Offer Columns to ItemVariant
-- Date: 2026-03-19
-- Description:
--   Add Offer, OfferStart, and OfferEnd columns to the ItemVariant table
--   to support the Manage Offers feature.
--   These columns are nullable to preserve existing rows.
--   A check constraint ensures Offer is between 0 and 100 when set.
-- =============================================

USE CanoEh;
GO

-- Add Offer column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ItemVariant') AND name = 'Offer')
BEGIN
    ALTER TABLE dbo.ItemVariant ADD Offer DECIMAL(5, 2) NULL;
    PRINT 'Added column Offer to ItemVariant';
END
ELSE
BEGIN
    PRINT 'Column Offer already exists in ItemVariant - skipping';
END
GO

-- Add OfferStart column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ItemVariant') AND name = 'OfferStart')
BEGIN
    ALTER TABLE dbo.ItemVariant ADD OfferStart DATETIME2 NULL;
    PRINT 'Added column OfferStart to ItemVariant';
END
ELSE
BEGIN
    PRINT 'Column OfferStart already exists in ItemVariant - skipping';
END
GO

-- Add OfferEnd column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ItemVariant') AND name = 'OfferEnd')
BEGIN
    ALTER TABLE dbo.ItemVariant ADD OfferEnd DATETIME2 NULL;
    PRINT 'Added column OfferEnd to ItemVariant';
END
ELSE
BEGIN
    PRINT 'Column OfferEnd already exists in ItemVariant - skipping';
END
GO

-- Add check constraint if it does not already exist
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ItemVariant_Offer' AND parent_object_id = OBJECT_ID('dbo.ItemVariant'))
BEGIN
    ALTER TABLE dbo.ItemVariant
        ADD CONSTRAINT CK_ItemVariant_Offer CHECK (Offer IS NULL OR (Offer >= 0 AND Offer <= 100));
    PRINT 'Added check constraint CK_ItemVariant_Offer';
END
ELSE
BEGIN
    PRINT 'Check constraint CK_ItemVariant_Offer already exists - skipping';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 012 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Added Offer column to ItemVariant (nullable decimal 0-100)';
PRINT '  - Added OfferStart column to ItemVariant (nullable datetime2)';
PRINT '  - Added OfferEnd column to ItemVariant (nullable datetime2)';
PRINT '  - Added check constraint CK_ItemVariant_Offer';
PRINT '==============================================';
GO
