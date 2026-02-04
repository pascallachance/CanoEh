-- =============================================
-- Migration: Refactor ItemAttribute and CategoryMandatoryAttribute
-- Date: 2026-02-04
-- Description: 
--   1. Move ItemAttribute from Item to ItemVariant (change FK from ItemID to ItemVariantID)
--   2. Rename ItemAttribute table to ItemVariantFeatures
--   3. Rename CategoryMandatoryAttribute table to CategoryMandatoryFeature
--   4. Drop ItemVariantExtraAttribute table (no longer needed)
--   5. Drop CategoryMandatoryExtraAttribute table (no longer needed)
-- =============================================

USE CanoEh;
GO

-- =============================================
-- Step 1: Drop ItemVariantExtraAttribute table
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemVariantExtraAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Drop foreign key constraint first
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ItemVariantExtraAttribute_ItemVariant')
    BEGIN
        ALTER TABLE dbo.ItemVariantExtraAttribute DROP CONSTRAINT FK_ItemVariantExtraAttribute_ItemVariant;
        PRINT 'Dropped FK constraint FK_ItemVariantExtraAttribute_ItemVariant';
    END

    DROP TABLE dbo.ItemVariantExtraAttribute;
    PRINT 'Dropped table ItemVariantExtraAttribute';
END
ELSE
BEGIN
    PRINT 'Table ItemVariantExtraAttribute does not exist - skipping';
END
GO

-- =============================================
-- Step 2: Drop CategoryMandatoryExtraAttribute table
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryMandatoryExtraAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Drop foreign key constraint first
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CategoryMandatoryExtraAttribute_CategoryNode')
    BEGIN
        ALTER TABLE dbo.CategoryMandatoryExtraAttribute DROP CONSTRAINT FK_CategoryMandatoryExtraAttribute_CategoryNode;
        PRINT 'Dropped FK constraint FK_CategoryMandatoryExtraAttribute_CategoryNode';
    END

    DROP TABLE dbo.CategoryMandatoryExtraAttribute;
    PRINT 'Dropped table CategoryMandatoryExtraAttribute';
END
ELSE
BEGIN
    PRINT 'Table CategoryMandatoryExtraAttribute does not exist - skipping';
END
GO

-- =============================================
-- Step 3: Rename ItemAttribute table to ItemVariantFeatures
--         and update FK from ItemID to ItemVariantID
-- =============================================

-- First, check if the table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Drop existing foreign key constraint
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ItemAttribute_Item')
    BEGIN
        ALTER TABLE dbo.ItemAttribute DROP CONSTRAINT FK_ItemAttribute_Item;
        PRINT 'Dropped FK constraint FK_ItemAttribute_Item';
    END

    -- Drop existing index
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ItemAttribute_ItemID' AND object_id = OBJECT_ID('dbo.ItemAttribute'))
    BEGIN
        DROP INDEX IX_ItemAttribute_ItemID ON dbo.ItemAttribute;
        PRINT 'Dropped index IX_ItemAttribute_ItemID';
    END

    -- Add new ItemVariantID column
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ItemAttribute') AND name = 'ItemVariantID')
    BEGIN
        ALTER TABLE dbo.ItemAttribute ADD ItemVariantID UNIQUEIDENTIFIER NULL;
        PRINT 'Added column ItemVariantID to ItemAttribute';
    END
END
GO

-- Continue Step 3: Migrate data from ItemID to ItemVariantID
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Migrate data: For each ItemAttribute, assign it to the first variant of its item
    -- Note: This is a best-effort migration. Manual review may be needed for multi-variant items.
    UPDATE ia
    SET ia.ItemVariantID = (
        SELECT TOP 1 iv.Id
        FROM dbo.ItemVariant iv
        WHERE iv.ItemId = ia.ItemID
        ORDER BY iv.Id
    )
    FROM dbo.ItemAttribute ia
    WHERE ia.ItemVariantID IS NULL AND ia.ItemID IS NOT NULL;
    PRINT 'Migrated ItemAttribute data from ItemID to ItemVariantID';

    -- Make ItemVariantID NOT NULL
    ALTER TABLE dbo.ItemAttribute ALTER COLUMN ItemVariantID UNIQUEIDENTIFIER NOT NULL;
    PRINT 'Made ItemVariantID NOT NULL';

    -- Drop old ItemID column
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ItemAttribute') AND name = 'ItemID')
    BEGIN
        ALTER TABLE dbo.ItemAttribute DROP COLUMN ItemID;
        PRINT 'Dropped column ItemID from ItemAttribute';
    END

    -- Create new foreign key constraint to ItemVariant
    ALTER TABLE dbo.ItemAttribute 
    ADD CONSTRAINT FK_ItemAttribute_ItemVariant 
    FOREIGN KEY (ItemVariantID) REFERENCES dbo.ItemVariant(Id);
    PRINT 'Added FK constraint FK_ItemAttribute_ItemVariant';

    -- Create new index
    CREATE INDEX IX_ItemAttribute_ItemVariantID ON dbo.ItemAttribute(ItemVariantID);
    PRINT 'Created index IX_ItemAttribute_ItemVariantID';

    -- Rename table to ItemVariantFeatures
    EXEC sp_rename 'dbo.ItemAttribute', 'ItemVariantFeatures';
    PRINT 'Renamed table ItemAttribute to ItemVariantFeatures';

    -- Rename foreign key constraint
    EXEC sp_rename 'FK_ItemAttribute_ItemVariant', 'FK_ItemVariantFeatures_ItemVariant', 'OBJECT';
    PRINT 'Renamed FK constraint to FK_ItemVariantFeatures_ItemVariant';

    -- Rename index
    EXEC sp_rename 'dbo.ItemVariantFeatures.IX_ItemAttribute_ItemVariantID', 'IX_ItemVariantFeatures_ItemVariantID', 'INDEX';
    PRINT 'Renamed index to IX_ItemVariantFeatures_ItemVariantID';
END
ELSE
BEGIN
    PRINT 'Table ItemAttribute does not exist - skipping ItemVariantFeatures migration';
END
GO

-- =============================================
-- Step 4: Rename CategoryMandatoryAttribute table to CategoryMandatoryFeature
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryMandatoryAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    -- Rename the table
    EXEC sp_rename 'dbo.CategoryMandatoryAttribute', 'CategoryMandatoryFeature';
    PRINT 'Renamed table CategoryMandatoryAttribute to CategoryMandatoryFeature';

    -- Rename foreign key constraint
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CategoryMandatoryAttribute_CategoryNode')
    BEGIN
        EXEC sp_rename 'FK_CategoryMandatoryAttribute_CategoryNode', 'FK_CategoryMandatoryFeature_CategoryNode', 'OBJECT';
        PRINT 'Renamed FK constraint to FK_CategoryMandatoryFeature_CategoryNode';
    END

    -- Rename indexes
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CategoryMandatoryAttribute_CategoryNodeId')
    BEGIN
        EXEC sp_rename 'dbo.CategoryMandatoryFeature.IX_CategoryMandatoryAttribute_CategoryNodeId', 'IX_CategoryMandatoryFeature_CategoryNodeId', 'INDEX';
        PRINT 'Renamed index to IX_CategoryMandatoryFeature_CategoryNodeId';
    END

    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CategoryMandatoryAttribute_SortOrder')
    BEGIN
        EXEC sp_rename 'dbo.CategoryMandatoryFeature.IX_CategoryMandatoryAttribute_SortOrder', 'IX_CategoryMandatoryFeature_SortOrder', 'INDEX';
        PRINT 'Renamed index to IX_CategoryMandatoryFeature_SortOrder';
    END
END
ELSE
BEGIN
    PRINT 'Table CategoryMandatoryAttribute does not exist - skipping CategoryMandatoryFeature migration';
END
GO

-- =============================================
-- Final Message
-- =============================================
PRINT '';
PRINT '==============================================';
PRINT 'Migration 007 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Dropped ItemVariantExtraAttribute table';
PRINT '  - Dropped CategoryMandatoryExtraAttribute table';
PRINT '  - Migrated ItemAttribute to ItemVariantFeatures';
PRINT '  - Changed FK from ItemID to ItemVariantID';
PRINT '  - Renamed CategoryMandatoryAttribute to CategoryMandatoryFeature';
PRINT '==============================================';
GO
