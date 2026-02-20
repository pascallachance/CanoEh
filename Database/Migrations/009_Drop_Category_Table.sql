-- =============================================
-- Migration: Drop old Category Table
-- =============================================
-- This migration removes the old Category table which has been
-- replaced by the CategoryNode hierarchy (Departement, Navigation, Category).
-- Migration 008 already updated the Item table to reference CategoryNode
-- instead of Category, so this table is now unused.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/009_Drop_Category_Table.sql"
-- =============================================

USE CanoEh;
GO

-- =============================================
-- Drop FK constraint referencing Category table (if still present)
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
-- Drop FK self-reference constraint on Category table
-- =============================================
IF EXISTS (
    SELECT * FROM sys.foreign_keys
    WHERE name = 'FK_Category_ParentCategory' AND parent_object_id = OBJECT_ID('dbo.Category')
)
BEGIN
    ALTER TABLE dbo.Category DROP CONSTRAINT FK_Category_ParentCategory;
    PRINT 'Dropped constraint FK_Category_ParentCategory.';
END
GO

-- =============================================
-- Drop index on Category table
-- =============================================
IF EXISTS (
    SELECT * FROM sys.indexes
    WHERE name = 'IX_Category_ParentCategoryId' AND object_id = OBJECT_ID('dbo.Category')
)
BEGIN
    DROP INDEX IX_Category_ParentCategoryId ON dbo.Category;
    PRINT 'Dropped index IX_Category_ParentCategoryId.';
END
GO

-- =============================================
-- Drop Category table
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Category' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    DROP TABLE dbo.Category;
    PRINT 'Dropped table Category.';
END
GO

PRINT 'Migration 009_Drop_Category_Table completed successfully.';
GO
