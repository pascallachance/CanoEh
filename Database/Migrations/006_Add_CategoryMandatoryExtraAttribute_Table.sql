-- =============================================
-- Migration: Add CategoryMandatoryExtraAttribute Table
-- =============================================
-- This migration adds the CategoryMandatoryExtraAttribute table to support
-- mandatory extra attributes (like SKU, Dimensions) that must be provided
-- when creating or editing item variants in a specific category.
--
-- Schema sync:
--   - The base schema script (Database/000_Create_Database_Schema.sql) should be
--     updated to include the dbo.CategoryMandatoryExtraAttribute table definition.
--
-- Migration history:
--   - A corresponding entry should be added to the migrations README/history for
--     006_Add_CategoryMandatoryExtraAttribute_Table, noting the addition of the
--     CategoryMandatoryExtraAttribute table and its indexes.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/006_Add_CategoryMandatoryExtraAttribute_Table.sql"
-- =============================================

USE CanoEh;
GO

-- =============================================
-- Create CategoryMandatoryExtraAttribute Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryMandatoryExtraAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.CategoryMandatoryExtraAttribute (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CategoryNodeId UNIQUEIDENTIFIER NOT NULL, -- FK to CategoryNode(Id), must be a CategoryNode
        Name_en NVARCHAR(100) NOT NULL,
        Name_fr NVARCHAR(100) NOT NULL,
        AttributeType NVARCHAR(50) NULL, -- e.g., 'string', 'int', 'enum', etc. (optional)
        SortOrder INT NULL,
        CONSTRAINT FK_CategoryMandatoryExtraAttribute_CategoryNode
            FOREIGN KEY (CategoryNodeId) REFERENCES dbo.CategoryNode(Id) ON DELETE CASCADE
    );
    
    -- Indexes for performance
    CREATE INDEX IX_CategoryMandatoryExtraAttribute_CategoryNodeId ON dbo.CategoryMandatoryExtraAttribute(CategoryNodeId);
    CREATE INDEX IX_CategoryMandatoryExtraAttribute_SortOrder ON dbo.CategoryMandatoryExtraAttribute(SortOrder);
    
    PRINT 'Table CategoryMandatoryExtraAttribute created successfully.';
END
ELSE
BEGIN
    PRINT 'Table CategoryMandatoryExtraAttribute already exists.';
END
GO

PRINT 'Migration 006_Add_CategoryMandatoryExtraAttribute_Table completed successfully.';
GO
