-- =============================================
-- Migration: Expand Item and OrderItem Name Columns to NVARCHAR(MAX)
-- Date: 2026-03-31
-- Description:
--   Alter Name_en and Name_fr on dbo.Item from NVARCHAR(255) to NVARCHAR(MAX)
--   so that sellers can enter product names of any length without hitting a
--   truncation error.  Description_en and Description_fr are already NVARCHAR(MAX)
--   and require no change.
--   Also expand dbo.OrderItem.Name_en and Name_fr to NVARCHAR(MAX) so that
--   placing an order for an item with a long name does not cause a truncation
--   error (OrderService copies Item.Name_* directly into OrderItem).
-- =============================================

USE CanoEh;
GO

-- =============================================
-- dbo.Item
-- =============================================

-- Expand Item.Name_en
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Name_en'
      AND max_length = 510   -- NVARCHAR(255) stores 255 chars × 2 bytes = 510
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Name_en NVARCHAR(MAX) NOT NULL;
    PRINT 'Column dbo.Item.Name_en expanded to NVARCHAR(MAX).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Name_en is already NVARCHAR(MAX) or does not exist - skipping.';
END
GO

-- Expand Item.Name_fr
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Item')
      AND name = 'Name_fr'
      AND max_length = 510   -- NVARCHAR(255) stores 255 chars × 2 bytes = 510
)
BEGIN
    ALTER TABLE dbo.Item
        ALTER COLUMN Name_fr NVARCHAR(MAX) NOT NULL;
    PRINT 'Column dbo.Item.Name_fr expanded to NVARCHAR(MAX).';
END
ELSE
BEGIN
    PRINT 'Column dbo.Item.Name_fr is already NVARCHAR(MAX) or does not exist - skipping.';
END
GO

-- =============================================
-- dbo.OrderItem
-- =============================================

-- Expand OrderItem.Name_en
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.OrderItem')
      AND name = 'Name_en'
      AND max_length = 510   -- NVARCHAR(255) stores 255 chars × 2 bytes = 510
)
BEGIN
    ALTER TABLE dbo.OrderItem
        ALTER COLUMN Name_en NVARCHAR(MAX) NOT NULL;
    PRINT 'Column dbo.OrderItem.Name_en expanded to NVARCHAR(MAX).';
END
ELSE
BEGIN
    PRINT 'Column dbo.OrderItem.Name_en is already NVARCHAR(MAX) or does not exist - skipping.';
END
GO

-- Expand OrderItem.Name_fr
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.OrderItem')
      AND name = 'Name_fr'
      AND max_length = 510   -- NVARCHAR(255) stores 255 chars × 2 bytes = 510
)
BEGIN
    ALTER TABLE dbo.OrderItem
        ALTER COLUMN Name_fr NVARCHAR(MAX) NOT NULL;
    PRINT 'Column dbo.OrderItem.Name_fr expanded to NVARCHAR(MAX).';
END
ELSE
BEGIN
    PRINT 'Column dbo.OrderItem.Name_fr is already NVARCHAR(MAX) or does not exist - skipping.';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 014 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Expanded Item.Name_en      to NVARCHAR(MAX) (was NVARCHAR(255))';
PRINT '  - Expanded Item.Name_fr      to NVARCHAR(MAX) (was NVARCHAR(255))';
PRINT '  - Expanded OrderItem.Name_en to NVARCHAR(MAX) (was NVARCHAR(255))';
PRINT '  - Expanded OrderItem.Name_fr to NVARCHAR(MAX) (was NVARCHAR(255))';
PRINT '  - Description_en / Description_fr on Item already NVARCHAR(MAX) - no change';
PRINT '==============================================';
GO
