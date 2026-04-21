-- =============================================
-- Migration: Add ItemReview Table
-- Date: 2026-04-21
-- Description:
--   Adds ItemReview table for product ratings/reviews.
--   - Ratings are constrained from 0 to 5 (maple leaves).
--   - One review per user per item.
-- =============================================

USE CanoEh;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemReview' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ItemReview (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ItemID UNIQUEIDENTIFIER NOT NULL,
        UserID UNIQUEIDENTIFIER NOT NULL,
        Rating INT NOT NULL,
        ReviewText NVARCHAR(2000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_ItemReview_Item FOREIGN KEY (ItemID) REFERENCES dbo.Item(Id),
        CONSTRAINT FK_ItemReview_User FOREIGN KEY (UserID) REFERENCES dbo.[User](Id),
        CONSTRAINT CK_ItemReview_Rating CHECK (Rating >= 0 AND Rating <= 5),
        CONSTRAINT UQ_ItemReview_Item_User UNIQUE (ItemID, UserID)
    );

    CREATE INDEX IX_ItemReview_ItemID ON dbo.ItemReview(ItemID);
    CREATE INDEX IX_ItemReview_UserID ON dbo.ItemReview(UserID);

    PRINT 'Created ItemReview table and indexes.';
END
ELSE
BEGIN
    PRINT 'ItemReview table already exists - skipping.';
END
GO

PRINT '';
PRINT '==============================================';
PRINT 'Migration 020 completed successfully!';
PRINT '==============================================';
PRINT 'Summary:';
PRINT '  - Added ItemReview table';
PRINT '  - Added rating check constraint (0 to 5)';
PRINT '  - Added unique key on (ItemID, UserID)';
PRINT '==============================================';
GO
