-- =============================================
-- Migration: Add CategoryNode Table
-- =============================================
-- This migration adds the CategoryNode table to support
-- the new hierarchical product category structure with:
-- - DepartementNode (root level)
-- - NavigationNode (intermediate grouping)
-- - CategoryNode (leaf categories for products)
--
-- The CategoryNode table will eventually replace the Category table.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/004_Add_CategoryNode_Table.sql"
-- =============================================

USE CanoEh;
GO

-- =============================================
-- Create CategoryNode Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryNode' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.CategoryNode (
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
        CONSTRAINT FK_CategoryNode_Parent FOREIGN KEY (ParentId) REFERENCES dbo.CategoryNode(Id),
        CONSTRAINT CK_CategoryNode_NodeType CHECK (NodeType IN ('Departement', 'Navigation', 'Category'))
    );
    
    -- Indexes for performance
    CREATE INDEX IX_CategoryNode_ParentId ON dbo.CategoryNode(ParentId);
    CREATE INDEX IX_CategoryNode_NodeType ON dbo.CategoryNode(NodeType);
    CREATE INDEX IX_CategoryNode_IsActive ON dbo.CategoryNode(IsActive);
    CREATE INDEX IX_CategoryNode_SortOrder ON dbo.CategoryNode(SortOrder);
    
    PRINT 'Table CategoryNode created successfully.';
END
ELSE
BEGIN
    PRINT 'Table CategoryNode already exists.';
END
GO

PRINT 'Migration 004_Add_CategoryNode_Table completed successfully.';
GO
