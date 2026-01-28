-- =============================================
-- CanoEh Database Validation Script
-- =============================================
-- This script verifies that the CanoEh database schema
-- has been created correctly with all required tables,
-- indexes, and constraints.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Validate_Database_Schema.sql"
-- =============================================

USE CanoEh;
GO

PRINT '';
PRINT '=============================================';
PRINT 'CanoEh Database Schema Validation';
PRINT '=============================================';
PRINT '';

-- =============================================
-- Check Database Exists
-- =============================================
IF DB_ID('CanoEh') IS NOT NULL
    PRINT '✓ Database CanoEh exists'
ELSE
BEGIN
    PRINT '✗ ERROR: Database CanoEh does not exist'
    RETURN
END
GO

-- =============================================
-- Validate Tables
-- =============================================
PRINT '';
PRINT 'Checking Tables:';
PRINT '----------------';

DECLARE @ExpectedTables TABLE (TableName NVARCHAR(100));
INSERT INTO @ExpectedTables VALUES 
    ('User'), ('Session'), ('Company'), ('Address'), ('PaymentMethod'),
    ('Category'), ('Item'), ('ItemVariant'), ('ItemAttribute'), ('ItemVariantAttribute'),
    ('Order'), ('OrderItem'), ('OrderAddress'), ('OrderPayment'),
    ('OrderStatus'), ('OrderItemStatus'), ('TaxRate'), ('ProductNode'), ('CategoryMandatoryAttribute');

DECLARE @TableName NVARCHAR(100);
DECLARE @TableExists BIT;
DECLARE @MissingTables INT = 0;

DECLARE TableCursor CURSOR FOR SELECT TableName FROM @ExpectedTables;
OPEN TableCursor;
FETCH NEXT FROM TableCursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT * FROM sys.tables WHERE name = @TableName AND schema_id = SCHEMA_ID('dbo'))
        PRINT '  ✓ ' + @TableName
    ELSE
    BEGIN
        PRINT '  ✗ MISSING: ' + @TableName
        SET @MissingTables = @MissingTables + 1
    END
    
    FETCH NEXT FROM TableCursor INTO @TableName;
END

CLOSE TableCursor;
DEALLOCATE TableCursor;

IF @MissingTables = 0
    PRINT '  All 19 tables created successfully!'
ELSE
    PRINT '  ERROR: ' + CAST(@MissingTables AS NVARCHAR) + ' table(s) missing'
GO

-- =============================================
-- Validate Foreign Keys
-- =============================================
PRINT '';
PRINT 'Checking Foreign Key Constraints:';
PRINT '----------------------------------';

DECLARE @ExpectedFKs INT = 18;
DECLARE @ActualFKs INT;

SELECT @ActualFKs = COUNT(*) 
FROM sys.foreign_keys 
WHERE schema_id = SCHEMA_ID('dbo');

IF @ActualFKs >= @ExpectedFKs
    PRINT '  ✓ Found ' + CAST(@ActualFKs AS NVARCHAR) + ' foreign key constraints'
ELSE
    PRINT '  ✗ Only found ' + CAST(@ActualFKs AS NVARCHAR) + ' foreign keys (expected at least ' + CAST(@ExpectedFKs AS NVARCHAR) + ')'
GO

-- =============================================
-- Validate Indexes
-- =============================================
PRINT '';
PRINT 'Checking Indexes:';
PRINT '-----------------';

DECLARE @IndexCount INT;
SELECT @IndexCount = COUNT(*) 
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.schema_id = SCHEMA_ID('dbo')
  AND i.type > 0  -- Exclude heaps
  AND i.is_primary_key = 0;  -- Exclude primary key indexes

PRINT '  ✓ Found ' + CAST(@IndexCount AS NVARCHAR) + ' non-primary key indexes'
GO

-- =============================================
-- Validate Lookup Tables Have Data
-- =============================================
PRINT '';
PRINT 'Checking Lookup Table Data:';
PRINT '----------------------------';

DECLARE @OrderStatusCount INT;
DECLARE @OrderItemStatusCount INT;

SELECT @OrderStatusCount = COUNT(*) FROM dbo.OrderStatus;
SELECT @OrderItemStatusCount = COUNT(*) FROM dbo.OrderItemStatus;

IF @OrderStatusCount >= 6
    PRINT '  ✓ OrderStatus has ' + CAST(@OrderStatusCount AS NVARCHAR) + ' records'
ELSE
    PRINT '  ✗ OrderStatus only has ' + CAST(@OrderStatusCount AS NVARCHAR) + ' records (expected at least 6)'

IF @OrderItemStatusCount >= 7
    PRINT '  ✓ OrderItemStatus has ' + CAST(@OrderItemStatusCount AS NVARCHAR) + ' records'
ELSE
    PRINT '  ✗ OrderItemStatus only has ' + CAST(@OrderItemStatusCount AS NVARCHAR) + ' records (expected at least 7)'
GO

-- =============================================
-- Validate Sequences
-- =============================================
PRINT '';
PRINT 'Checking Sequences:';
PRINT '-------------------';

IF EXISTS (SELECT * FROM sys.sequences WHERE name = 'OrderNumberSequence')
    PRINT '  ✓ OrderNumberSequence exists'
ELSE
    PRINT '  ✗ MISSING: OrderNumberSequence'
GO

-- =============================================
-- Validate Unique Constraints
-- =============================================
PRINT '';
PRINT 'Checking Unique Constraints:';
PRINT '----------------------------';

DECLARE @UniqueConstraints INT;
SELECT @UniqueConstraints = COUNT(*) 
FROM sys.key_constraints
WHERE type = 'UQ' AND schema_id = SCHEMA_ID('dbo');

IF @UniqueConstraints >= 4
    PRINT '  ✓ Found ' + CAST(@UniqueConstraints AS NVARCHAR) + ' unique constraints'
ELSE
    PRINT '  ✗ Only found ' + CAST(@UniqueConstraints AS NVARCHAR) + ' unique constraints (expected at least 4)'
GO

-- =============================================
-- List Column Counts
-- =============================================
PRINT '';
PRINT 'Table Column Counts:';
PRINT '--------------------';

SELECT 
    t.name AS TableName,
    COUNT(c.column_id) AS ColumnCount
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.schema_id = SCHEMA_ID('dbo')
GROUP BY t.name
ORDER BY t.name;
GO

-- =============================================
-- Summary
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'Validation Complete';
PRINT '=============================================';
PRINT '';
PRINT 'Review the results above. All items should';
PRINT 'show a ✓ checkmark. If any show ✗, review';
PRINT 'the creation script and re-run as needed.';
PRINT '';
GO
