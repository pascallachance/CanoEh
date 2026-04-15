-- =============================================
-- Migration: Add OfferMaxBuyQty To ItemVariant
-- Date: 2026-04-15
-- Description:
--   Add OfferMaxBuyQty column to the ItemVariant table
--   to allow sellers to limit the number of items a user
--   can buy in a single transaction during an offer.
--   The column is nullable (no limit when NULL) and must
--   be greater than 0 when set.
-- =============================================

USE CanoEh;
GO

-- Add OfferMaxBuyQty column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ItemVariant') AND name = 'OfferMaxBuyQty')
BEGIN
    ALTER TABLE dbo.ItemVariant ADD OfferMaxBuyQty INT NULL;
    PRINT 'Added column OfferMaxBuyQty to ItemVariant';
END
ELSE
BEGIN
    PRINT 'Column OfferMaxBuyQty already exists in ItemVariant - skipping';
END
GO

-- Add check constraint to ensure OfferMaxBuyQty > 0 when set
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_ItemVariant_OfferMaxBuyQty' AND parent_object_id = OBJECT_ID('dbo.ItemVariant'))
BEGIN
    ALTER TABLE dbo.ItemVariant
        ADD CONSTRAINT CK_ItemVariant_OfferMaxBuyQty CHECK (OfferMaxBuyQty IS NULL OR OfferMaxBuyQty > 0);
    PRINT 'Added check constraint CK_ItemVariant_OfferMaxBuyQty';
END
ELSE
BEGIN
    PRINT 'Check constraint CK_ItemVariant_OfferMaxBuyQty already exists - skipping';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 019 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Added OfferMaxBuyQty column to ItemVariant (nullable int > 0)';
PRINT '  - Added check constraint CK_ItemVariant_OfferMaxBuyQty';
PRINT '==============================================';
GO
