-- =============================================
-- Migration: Add CategoryMandatoryAttribute Table
-- =============================================
-- This migration adds the CategoryMandatoryAttribute table to support
-- mandatory attributes that must be provided when creating or editing
-- products in a specific category.
--
-- Schema sync:
--   - The base schema script (Database/000_Create_Database_Schema.sql) has been
--     updated to include the dbo.CategoryMandatoryAttribute table definition.
--
-- Migration history:
--   - A corresponding entry has been added to the migrations README/history for
--     005_Add_CategoryMandatoryAttribute_Table, noting the addition of the
--     CategoryMandatoryAttribute table and its indexes.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/005_Add_CategoryMandatoryAttribute_Table.sql"
-- =============================================

USE CanoEh;
GO

-- =============================================
-- Create CategoryMandatoryAttribute Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryMandatoryAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.CategoryMandatoryAttribute (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CategoryNodeId UNIQUEIDENTIFIER NOT NULL, -- FK to ProductNode(Id), must be a CategoryNode
        Name_en NVARCHAR(100) NOT NULL,
        Name_fr NVARCHAR(100) NOT NULL,
        AttributeType NVARCHAR(50) NULL, -- e.g., 'string', 'int', 'enum', etc. (optional)
        SortOrder INT NULL,
        CONSTRAINT FK_CategoryMandatoryAttribute_ProductNode
            FOREIGN KEY (CategoryNodeId) REFERENCES dbo.ProductNode(Id) ON DELETE CASCADE
    );
    
    -- Indexes for performance
    CREATE INDEX IX_CategoryMandatoryAttribute_CategoryNodeId ON dbo.CategoryMandatoryAttribute(CategoryNodeId);
    CREATE INDEX IX_CategoryMandatoryAttribute_SortOrder ON dbo.CategoryMandatoryAttribute(SortOrder);
    
    PRINT 'Table CategoryMandatoryAttribute created successfully.';
END
ELSE
BEGIN
    PRINT 'Table CategoryMandatoryAttribute already exists.';
END
GO

PRINT 'Migration 005_Add_CategoryMandatoryAttribute_Table completed successfully.';
GO
