-- =============================================
-- Migration: Rename CategoryID to CategoryNodeID in Item Table
-- =============================================
-- This migration updates the Item table to reference CategoryNode
-- instead of Category, replacing the old Category table FK with
-- a FK to the CategoryNode table.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/008_Rename_CategoryID_To_CategoryNodeID.sql"
-- =============================================

USE CanoEh;
GO

-- =============================================
-- Drop FK constraint referencing Category table
-- =============================================
IF EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE name = 'FK_Item_Category' AND parent_object_id = OBJECT_ID('dbo.Item')
)
BEGIN
    ALTER TABLE dbo.Item DROP CONSTRAINT FK_Item_Category;
    PRINT 'Dropped constraint FK_Item_Category.';
END
GO

-- =============================================
-- Drop old index on CategoryID
-- =============================================
IF EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Item_CategoryID' AND object_id = OBJECT_ID('dbo.Item')
)
BEGIN
    DROP INDEX IX_Item_CategoryID ON dbo.Item;
    PRINT 'Dropped index IX_Item_CategoryID.';
END
GO

-- =============================================
-- Rename column CategoryID to CategoryNodeID
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE name = 'CategoryID' AND object_id = OBJECT_ID('dbo.Item')
)
BEGIN
    EXEC sp_rename 'dbo.Item.CategoryID', 'CategoryNodeID', 'COLUMN';
    PRINT 'Renamed column CategoryID to CategoryNodeID.';
END
GO

-- =============================================
-- Add FK constraint referencing CategoryNode table
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE name = 'FK_Item_CategoryNode' AND parent_object_id = OBJECT_ID('dbo.Item')
)
BEGIN
    ALTER TABLE dbo.Item
        ADD CONSTRAINT FK_Item_CategoryNode
        FOREIGN KEY (CategoryNodeID) REFERENCES dbo.CategoryNode(Id);
    PRINT 'Added constraint FK_Item_CategoryNode.';
END
GO

-- =============================================
-- Add new index on CategoryNodeID
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Item_CategoryNodeID' AND object_id = OBJECT_ID('dbo.Item')
)
BEGIN
    CREATE INDEX IX_Item_CategoryNodeID ON dbo.Item(CategoryNodeID);
    PRINT 'Created index IX_Item_CategoryNodeID.';
END
GO

PRINT 'Migration 008_Rename_CategoryID_To_CategoryNodeID completed successfully.';
GO
