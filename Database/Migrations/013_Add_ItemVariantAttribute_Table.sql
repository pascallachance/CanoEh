-- =============================================
-- Migration: Add ItemVariantAttribute Table
-- Date: 2026-03-19
-- Description:
--   Create the ItemVariantAttribute table if it does not already exist.
--   This table was added directly to the base schema without a migration,
--   so databases provisioned before the table was introduced are missing it.
--   The DELETE and INSERT on dbo.ItemVariantAttribute in UpdateItemAsync
--   throw SqlException: "Invalid object name 'dbo.ItemVariantAttribute'"
--   on those older databases, causing a 500 on every product edit.
-- =============================================

USE CanoEh;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemVariantAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ItemVariantAttribute (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ItemVariantID UNIQUEIDENTIFIER NOT NULL,
        AttributeName_en NVARCHAR(255) NOT NULL,
        AttributeName_fr NVARCHAR(255) NULL,
        Attributes_en NVARCHAR(MAX) NOT NULL,
        Attributes_fr NVARCHAR(MAX) NULL,
        CONSTRAINT FK_ItemVariantAttribute_ItemVariant FOREIGN KEY (ItemVariantID) REFERENCES dbo.ItemVariant(Id)
    );

    CREATE INDEX IX_ItemVariantAttribute_ItemVariantID ON dbo.ItemVariantAttribute(ItemVariantID);

    PRINT 'Table ItemVariantAttribute created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ItemVariantAttribute already exists - skipping';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 013 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Created ItemVariantAttribute table (if absent)';
PRINT '  - Added FK constraint FK_ItemVariantAttribute_ItemVariant';
PRINT '  - Added index IX_ItemVariantAttribute_ItemVariantID';
PRINT '==============================================';
GO
