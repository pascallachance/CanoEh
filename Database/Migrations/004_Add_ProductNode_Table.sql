-- =============================================
-- Migration: Add ProductNode Table
-- =============================================
-- This migration adds the ProductNode table to support
-- the new hierarchical product structure with:
-- - DepartementNode (root level)
-- - NavigationNode (intermediate grouping)
-- - CategoryNode (leaf categories for products)
--
-- The ProductNode table will eventually replace the Category table.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/004_Add_ProductNode_Table.sql"
-- =============================================

USE CanoEh;
GO

-- =============================================
-- Create ProductNode Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductNode' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ProductNode (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name_en NVARCHAR(200) NOT NULL,
        Name_fr NVARCHAR(200) NOT NULL,
        NodeType NVARCHAR(32) NOT NULL, -- 'Departement', 'Navigation', 'Category'
        ParentId UNIQUEIDENTIFIER NULL, -- Self-reference to parent node
        -- Example: for CategoryNode, ParentId points to a NavigationNode or DepartementNode
        --          for NavigationNode, ParentId points to a DepartementNode or another NavigationNode
        --          for DepartementNode, ParentId is NULL (root)
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_ProductNode_Parent FOREIGN KEY (ParentId) REFERENCES dbo.ProductNode(Id),
        CONSTRAINT CK_ProductNode_NodeType CHECK (NodeType IN ('Departement', 'Navigation', 'Category'))
    );
    
    -- Indexes for performance
    CREATE INDEX IX_ProductNode_ParentId ON dbo.ProductNode(ParentId);
    CREATE INDEX IX_ProductNode_NodeType ON dbo.ProductNode(NodeType);
    CREATE INDEX IX_ProductNode_IsActive ON dbo.ProductNode(IsActive);
    CREATE INDEX IX_ProductNode_SortOrder ON dbo.ProductNode(SortOrder);
    
    PRINT 'Table ProductNode created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ProductNode already exists.';
END
GO

PRINT 'Migration 004_Add_ProductNode_Table completed successfully.';
GO
